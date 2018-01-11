using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Lime;
using Tangerine.Core.Components;

namespace Tangerine.Core
{
	public interface IDocumentView
	{
		void Detach();
		void Attach();
	}

	public enum DocumentFormat
	{
		Scene,
		Tan,
		Model
	}

	public sealed class Document
	{
		public enum CloseAction
		{
			Cancel,
			SaveChanges,
			DiscardChanges
		}

		public static readonly string[] AllowedFileTypes = { "scene", "tan", "model" };

		readonly string defaultPath = "Untitled";
		readonly Vector2 defaultSceneSize = new Vector2(1024, 768);

		public delegate bool PathSelectorDelegate(out string path);

		private readonly Dictionary<object, Row> rowCache = new Dictionary<object, Row>();

		private DateTime modificationTime;

		public static event Action<Document> AttachingViews;
		public static Func<Document, CloseAction> CloseConfirmation;
		public static PathSelectorDelegate PathSelector;

		public static Document Current { get; private set; }

		public readonly DocumentHistory History = new DocumentHistory();
		public bool IsModified => History.IsDocumentModified;

		/// <summary>
		/// The list of Tangerine node decorators.
		/// </summary>
		public static readonly NodeDecoratorList NodeDecorators = new NodeDecoratorList();

		/// <summary>
		/// Gets the path to the document relative to the project directory.
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// Gets or sets the file format the document should be saved to.
		/// </summary>
		public DocumentFormat Format { get; set; }

		/// <summary>
		/// Gets the root node for the current document.
		/// </summary>
		public Node RootNode { get; private set; }

		/// <summary>
		/// Gets or sets the current container widget.
		/// </summary>
		public Node Container { get; set; }

		/// <summary>
		/// Gets or sets the scene we are navigated from. Need for getting back into the main scene from the external one.
		/// </summary>
		public string SceneNavigatedFrom { get; set; }

		/// <summary>
		/// The list of rows, currently displayed on the timeline.
		/// </summary>
		public readonly List<Row> Rows = new List<Row>();

		/// <summary>
		/// The root of the current row hierarchy.
		/// </summary>
		public Row RowTree { get; set; }

		/// <summary>
		/// The list of views (timeline, inspector, ...)
		/// </summary>
		public readonly List<IDocumentView> Views = new List<IDocumentView>();

		public int AnimationFrame
		{
			get { return Container.AnimationFrame; }
			set { Container.AnimationFrame = value; }
		}

		public bool PreviewAnimation { get; set; }
		public int PreviewAnimationBegin { get; set; }
		public Node PreviewAnimationContainer { get; set; }
		public bool ExpositionMode { get; set; }

		public string AnimationId { get; set; }

		public Document()
		{
			Format = DocumentFormat.Scene;
			Path = defaultPath;
			Container = RootNode = new Frame { Size = defaultSceneSize };
			RootNode.DefaultAnimation.AnimationEngine = new Orange.CompatibilityAnimationEngine();
			SetModificationTimeToNow();
		}

		public Document(string path)
		{
			try {
				Path = path;
				Format = ResolveFormat(path);
				RootNode = Node.CreateFromAssetBundle(path);
				SetModificationTimeToNow();
				if (RootNode is Node3D) {
					RootNode = WrapNodeWithViewport3D(RootNode);
				}
				Decorate(RootNode);
				Container = RootNode;
			} catch (System.Exception e) {
				throw new System.InvalidOperationException($"Can't open '{path}': {e.Message}");
			}
		}

		Viewport3D WrapNodeWithViewport3D(Node node)
		{
			var vp = new Viewport3D { Width = 1024, Height = 768 };
			vp.AddNode(node);
			var camera = new Camera3D {
				Id = "DefaultCamera",
				Position = new Vector3(0, 0, 10),
				FarClipPlane = 1000,
				NearClipPlane = 0.01f,
				FieldOfView = 1.0f,
				AspectRatio = 1.3f,
				OrthographicSize = 1.0f
			};
			vp.AddNode(camera);
			vp.CameraRef = new NodeReference<Camera3D>(camera.Id);
			return vp;
		}

		public bool WasModifiedOutsideTangerine()
		{
			if (Path == defaultPath) {
				return false;
			}
			var fullPath = Project.Current.GetSystemPath(Path, GetFileExtension(Format));
			return File.GetLastWriteTimeUtc(fullPath) > modificationTime;
		}

		public void SetModificationTimeToNow()
		{
			modificationTime = DateTime.UtcNow;
		}

		static DocumentFormat ResolveFormat(string path)
		{
			if (AssetExists(path, "scene")) {
				return DocumentFormat.Scene;
			} else if (AssetExists(path, "tan")) {
				return DocumentFormat.Tan;
			} else if (AssetExists(path, "model")) {
				return DocumentFormat.Model;
			} else {
				throw new FileNotFoundException(path);
			}
		}

		public static string GetFileExtension(DocumentFormat format)
		{
			switch (format) {
				case DocumentFormat.Model: return "model";
				case DocumentFormat.Scene: return "scene";
				case DocumentFormat.Tan: return "tan";
				default: throw new InvalidOperationException();
			}
		}

		public string GetFileExtension() => GetFileExtension(Format);

		static bool AssetExists(string path, string ext) => AssetBundle.Current.FileExists(System.IO.Path.ChangeExtension(path, ext));

		public void MakeCurrent()
		{
			SetCurrent(this);
		}

		public static void SetCurrent(Document doc)
		{
			if (Current != doc) {
				Current?.DetachViews();
				Current = doc;
				doc?.AttachViews();
			}
		}

		void AttachViews()
		{
			RefreshExternalScenes();
			AttachingViews?.Invoke(this);
			foreach (var i in Current.Views) {
				i.Attach();
			}
			SelectFirstRowIfNoneSelected();
		}

		private void SelectFirstRowIfNoneSelected()
		{
			if (!SelectedRows().Any()) {
				Operations.Dummy.Perform();
				if (Rows.Count > 0) {
					Operations.SelectRow.Perform(Rows[0]);
				}
			}
		}

		public void RefreshExternalScenes() => RefreshExternalScenes(RootNode);

		public void RefreshExternalScenes(Node node)
		{
			if (!string.IsNullOrEmpty(node.ContentsPath)) {
				var doc = Project.Current.Documents.FirstOrDefault(i => i.Path == node.ContentsPath);
				if (doc != null && doc.IsModified) {
					var docRootNode = doc.RootNode is Viewport3D ? doc.RootNode.Nodes[0] : doc.RootNode;
					node.ReplaceContent(docRootNode.Clone());
				} else {
					node.LoadExternalScenes();
				}
			}
			foreach (var child in node.Nodes) {
				RefreshExternalScenes(child);
			}
		}

		void DetachViews()
		{
			foreach (var i in Current.Views) {
				i.Detach();
			}
		}

		public bool Close()
		{
			if (!IsModified) {
				return true;
			}
			if (CloseConfirmation != null) {
				var r = CloseConfirmation(this);
				if (r == CloseAction.Cancel) {
					return false;
				}
				if (r == CloseAction.SaveChanges) {
					Save();
				}
			} else {
				Save();
			}
			return true;
		}

		public void Save()
		{
			if (Path == defaultPath) {
				string path;
				if (PathSelector(out path)) {
					SaveAs(path);
				}
			} else {
				SaveAs(Path);
			}
		}

		public void SaveAs(string path)
		{
			History.AddSavePoint();
			Path = path;
			WriteNodeToFile(path, Format, RootNode);
			SetModificationTimeToNow();
		}

		public static void WriteNodeToFile(string path, DocumentFormat format, Node node)
		{
			// Save the document into memory at first to avoid a torn file in the case of a serialization error.
			var ms = new MemoryStream();
			// Dispose cloned object to preserve keyframes identity in the original node. See Animator.Dispose().
			using (node = CreateCloneForSerialization(node)) {
				if (format == DocumentFormat.Scene) {
					var serializer = new HotSceneSerializer();
					Serialization.WriteObject(path, ms, node, serializer);
				} else {
					Serialization.WriteObject(path, ms, node, Serialization.Format.JSON);
				}
			}
			var fullPath = Project.Current.GetSystemPath(path, GetFileExtension(format));
			using (var fs = new FileStream(fullPath, FileMode.Create)) {
				var a = ms.ToArray();
				fs.Write(a, 0, a.Length);
			}
		}

		public static Node CreateCloneForSerialization(Node node)
		{
			var clone = node.Clone();
			Action<Node> f = (n) => {
				n.AnimationFrame = 0;
				if (n.Folders != null && n.Folders.Count == 0) {
					n.Folders = null;
				}
				foreach (var a in n.Animators.ToList()) {
					if (a.ReadonlyKeys.Count == 0) {
						n.Animators.Remove(a);
					}
				}
				if (!string.IsNullOrEmpty(n.ContentsPath)) {
					n.Nodes.Clear();
					n.Markers.Clear();
				}
				if (n.AsWidget?.SkinningWeights?.IsEmpty() ?? false) {
					n.AsWidget.SkinningWeights = null;
				} else if ((n as PointObject)?.SkinningWeights?.IsEmpty() ?? false) {
					(n as PointObject).SkinningWeights = null;
				}
			};
			f(clone);
			foreach (var n in clone.Descendants) {
				f(n);
			}
			return clone;
		}

		public IEnumerable<Row> SelectedRows()
		{
			foreach (var row in Rows) {
				if (row.Selected) {
					yield return row;
				}
			}
		}

		public IEnumerable<Node> SelectedNodes()
		{
			Node prevNode = null;
			foreach (var row in Rows) {
				if (row.Selected) {
					var nr = row.Components.Get<NodeRow>();
					if (nr != null) {
						yield return nr.Node;
						prevNode = nr.Node;
					}
					var pr = row.Components.Get<PropertyRow>();
					if (pr != null && pr.Node != prevNode) {
						yield return pr.Node;
						prevNode = pr.Node;
					}
				}
			}
		}

		public IEnumerable<IFolderItem> SelectedFolderItems()
		{
			foreach (var row in Rows) {
				if (row.Selected) {
					var nr = row.Components.Get<NodeRow>();
					if (nr != null) {
						yield return nr.Node;
					}
					var fr = row.Components.Get<FolderRow>();
					if (fr != null) {
						yield return fr.Folder;
					}
				}
			}
		}

		public IEnumerable<Row> TopLevelSelectedRows()
		{
			foreach (var row in Rows) {
				if (row.Selected) {
					var discardRow = false;
					for (var p = row.Parent; p != null; p = p.Parent) {
						discardRow |= p.Selected;
					}
					if (!discardRow) {
						yield return row;
					}
				}
			}
		}

		public Row GetRowForObject(object obj)
		{
			Row row;
			if (!rowCache.TryGetValue(obj, out row)) {
				row = new Row();
				rowCache.Add(obj, row);
			}
			return row;
		}

		public static bool HasCurrent() => Current != null;

		public void Decorate(Node node)
		{
			if (Format == DocumentFormat.Scene) {
				node.DefaultAnimation.AnimationEngine = new Orange.CompatibilityAnimationEngine();
			}
			foreach (var decorator in NodeDecorators) {
				decorator(node);
				foreach (var descendant in node.Descendants) {
					decorator(descendant);
				}
			}
		}

		public class NodeDecoratorList : List<Action<Node>>
		{
			public void AddFor<T>(Action<Node> action) where T: Node
			{
				Add(node => {
					if (node is T) {
						action(node);
					}
				});
			}
		}
	}
}
