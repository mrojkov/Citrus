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
		T3D,
		Fbx
	}

	public enum CompoundAnimationInspectionMode
	{
		Tracks,
		Clips,
	}

	public sealed partial class Document
	{
		public enum CloseAction
		{
			Cancel,
			SaveChanges,
			DiscardChanges
		}

		public static readonly string[] AllowedFileTypes = { "scene", "tan", "t3d", "fbx" };

		private readonly string defaultPath = "Untitled";
		private readonly Vector2 defaultSceneSize = new Vector2(1024, 768);

		public delegate bool PathSelectorDelegate(out string path);

		private readonly Dictionary<object, Row> rowCache = new Dictionary<object, Row>();

		private readonly MemoryStream preloadedSceneStream = null;
		public DateTime LastWriteTime { get; private set; }

		public bool Loaded { get; private set; } = true;

		public static event Action<Document> AttachingViews;
		public static Func<Document, CloseAction> CloseConfirmation;
		public static PathSelectorDelegate PathSelector;

		public static Document Current { get; private set; }
		public static Document Clicked { get; set; }

		public readonly DocumentHistory History = new DocumentHistory();
		public bool IsModified => History.IsDocumentModified;
		public event Action<Document> Saving;

		public CompoundAnimationInspectionMode GetCompoundAnimationIspectMode = CompoundAnimationInspectionMode.Tracks;

		/// <summary>
		/// The list of Tangerine node decorators.
		/// </summary>
		public static readonly NodeDecoratorList NodeDecorators = new NodeDecoratorList();

		/// <summary>
		/// Gets the path to the document relative to the project directory.
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// Gets absolute path to the document.
		/// </summary>
		public string FullPath => Project.Current.GetFullPath(Path, GetFileExtension(Format));

		/// <summary>
		/// Document name to be displayed.
		/// </summary>
		public string DisplayName => (IsModified ? "*" : string.Empty) + System.IO.Path.GetFileName(Path ?? "Untitled");

		/// <summary>
		/// Gets or sets the file format the document should be saved to.
		/// </summary>
		public DocumentFormat Format { get; set; }

		public Node RootNodeUnwrapped { get; private set; }

		/// <summary>
		/// Gets the root node for the current document.
		/// </summary>
		public Node RootNode { get; private set; }

		private Node container;

		/// <summary>
		/// Gets or sets the current container widget.
		/// </summary>
		public Node Container
		{
			get => container;
			set {
				if (container != value) {
					container = value;
					var animations = new List<Animation>();
					GetAnimations(animations);
					if (!animations.Contains(Animation)) {
						SelectedAnimation = null;
					}
				}
			}
		}

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

		/// <summary>
		/// Base64 representation of Document preview in .png format
		/// </summary>
		public string Preview { get; set; }

		public int AnimationFrame
		{
			get => Animation.Frame;
			set => Animation.Frame = value;
		}

		public bool PreviewAnimation { get; set; }
		public int PreviewAnimationBegin { get; set; }
		public Node PreviewAnimationContainer { get; set; }
		public bool ExpositionMode { get; set; }
		public ResolutionPreview ResolutionPreview { get; set; } = new ResolutionPreview();
		public bool InspectRootNode { get; set; }

		public Animation Animation => SelectedAnimation ?? Container.DefaultAnimation;

		public Animation SelectedAnimation { get; set; }

		public string AnimationId => Animation.Id;

		public Document(DocumentFormat format = DocumentFormat.Scene, Type rootType = null)
		{
			Format = format;
			Path = defaultPath;
			if (rootType == null) {
				Container = RootNodeUnwrapped = RootNode = new Frame { Size = defaultSceneSize };
			} else {
				var constructor = rootType.GetConstructor(Type.EmptyTypes);
				Container = RootNodeUnwrapped = RootNode = (Node)constructor.Invoke(new object[] { });
				if (RootNode is Widget widget) {
					widget.Size = defaultSceneSize;
				}
			}
			RootNode = RootNodeUnwrapped;
			if (RootNode is Node3D) {
				RootNode = WrapNodeWithViewport3D(RootNode);
			}
			Decorate(RootNode);
			Container = RootNode;
			History.PerformingOperation += Document_PerformingOperation;
			History.Changed += Document_Changed;
		}

		public Document(string path, bool delayLoad = false)
		{
			Path = path;
			Loaded = false;
			Format = ResolveFormat(Path);
			LastWriteTime = File.GetLastWriteTime(FullPath);
			if (delayLoad) {
				preloadedSceneStream = new MemoryStream();
				var fullPath = Node.ResolveScenePath(path);
				using (var stream = AssetBundle.Current.OpenFileLocalized(fullPath)) {
					stream.CopyTo(preloadedSceneStream);
					preloadedSceneStream.Seek(0, SeekOrigin.Begin);
				}
			} else {
				Load();
			}
		}

		public void GetAnimations(List<Animation> animations)
		{
			var ancestor = Container;
			animations.Add(ancestor.DefaultAnimation);
			while (true) {
				foreach (var a in ancestor.TangerineAnimations) {
					var found = false;
					foreach (var other in animations) {
						found = other.Id == a.Id;
						if (found) {
							break;
						}
					}
					if (!found) {
						animations.Add(a);
					}
				}
				if (ancestor == RootNode) {
					return;
				}
				ancestor = ancestor.Parent;
			}
		}

		private void Load()
		{
			try {
				if (preloadedSceneStream != null) {
					RootNodeUnwrapped = Node.CreateFromStream(Path + $".{GetFileExtension(Format)}", yuzu: TangerineYuzu.Instance.Value, stream: preloadedSceneStream);
				} else {
					RootNodeUnwrapped = Node.CreateFromAssetBundle(Path, yuzu: TangerineYuzu.Instance.Value);
				}
				if (Format == DocumentFormat.Fbx) {
					Path = defaultPath;
				}
				RootNode = RootNodeUnwrapped;
				if (RootNode is Node3D) {
					RootNode = WrapNodeWithViewport3D(RootNode);
				}
				Decorate(RootNode);
				Container = RootNode;
				if (Format == DocumentFormat.Scene || Format == DocumentFormat.Tan) {
					if (preloadedSceneStream != null) {
						preloadedSceneStream.Seek(0, SeekOrigin.Begin);
						Preview = DocumentPreview.ReadAsBase64(preloadedSceneStream);
					} else {
						Preview = DocumentPreview.ReadAsBase64(FullPath);
					}
				}
				History.PerformingOperation += Document_PerformingOperation;
				History.Changed += Document_Changed;
			} catch (System.Exception e) {
				throw new System.InvalidOperationException($"Can't open '{Path}': {e.Message}");
			}
			Loaded = true;
		}

		private void Document_Changed() => Project.Current.SceneCache.InvalidateEntryFromOpenedDocumentChanged(Path, IsModified ? (Func<Node>)(() => RootNodeUnwrapped) : null);

		private void Document_PerformingOperation(IOperation operation)
		{
			if (PreviewAnimation) {
				TogglePreviewAnimation(!PreviewAnimation, triggerMarkersBeforeCurrentFrame: true);
			}
		}

		private static Viewport3D WrapNodeWithViewport3D(Node node)
		{
			var vp = new Viewport3D { Width = 1024, Height = 768 };
			vp.AddNode(node);
			var camera = node.Descendants.FirstOrDefault(n => n is Camera3D);
			if (camera == null) {
				camera = new Camera3D {
					Id = "DefaultCamera",
					Position = new Vector3(0, 0, 10),
					FarClipPlane = 1000,
					NearClipPlane = 0.01f,
					FieldOfView = 1.0f,
					AspectRatio = 1.3f,
					OrthographicSize = 1.0f
				};
				vp.AddNode(camera);
			}
			vp.CameraRef = new NodeReference<Camera3D>(camera.Id);
			return vp;
		}

		public static DocumentFormat ResolveFormat(string path)
		{
			if (AssetExists(path, "scene")) {
				return DocumentFormat.Scene;
			}
			if (AssetExists(path, "tan")) {
				return DocumentFormat.Tan;
			}
			if (AssetExists(path, "fbx")) {
				return DocumentFormat.Fbx;
			}
			if (AssetExists(path, "t3d")) {
				return DocumentFormat.T3D;
			}
			throw new FileNotFoundException(path);
		}

		public static string GetFileExtension(DocumentFormat format)
		{
			switch (format) {
				case DocumentFormat.Scene:
					return "scene";
				case DocumentFormat.Tan:
					return "tan";
				case DocumentFormat.T3D:
				case DocumentFormat.Fbx:
					return "t3d";
				default: throw new InvalidOperationException();
			}
		}

		public string GetFileExtension() => GetFileExtension(Format);

		static bool AssetExists(string path, string ext) => AssetBundle.Current.FileExists(path + $".{ext}");

		public void MakeCurrent()
		{
			SetCurrent(this);
		}

		public static void SetCurrent(Document doc)
		{
			if (!(doc?.Loaded ?? true)) {
				if (Project.Current.GetFullPath(doc.Path, out string fullPath) || doc.preloadedSceneStream != null) {
					doc.Load();
				}
			}
			if (Current != doc) {
				DetachViews();
				Current = doc;
				doc?.AttachViews();
				if (doc != null) {
					ProjectUserPreferences.Instance.CurrentDocument = doc.Path;
				}
				ForceAnimationUpdate();
			}
		}

		private void AttachViews()
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
				using (History.BeginTransaction()) {
					Operations.Dummy.Perform(Current.History);
					if (Rows.Count > 0) {
						Operations.SelectRow.Perform(Rows[0]);
					}
					History.CommitTransaction();
				}
			}
		}

		public void RefreshExternalScenes() => RootNode.LoadExternalScenes();

		private static void DetachViews()
		{
			if (Current == null) {
				return;
			}
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
				if (PathSelector(out var path)) {
					SaveAs(path);
				}
			} else {
				SaveAs(Path);
			}
		}

		public void SaveAs(string path)
		{
			Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FullPath));
			if (!Loaded && IsModified) {
				Load();
			}
			Project.RaiseDocumentSaving(this);
			Saving?.Invoke(this);
			History.AddSavePoint();
			Path = path;
			var needSupressCreatedEvent = !File.Exists(FullPath);
			WriteNodeToFile(path, Format, RootNodeUnwrapped);
			if (Format == DocumentFormat.Scene || Format == DocumentFormat.Tan) {
				DocumentPreview.AppendToFile(FullPath, Preview);
			}
			LastWriteTime = File.GetLastWriteTime(FullPath);
			Project.Current.AddRecentDocument(Path);
		}

		public void SaveTo(string path, FileAttributes attributes = 0)
		{
			WriteNodeToFile(path, Format, RootNodeUnwrapped, attributes);
		}

		public static void WriteNodeToFile(string path, DocumentFormat format, Node node, FileAttributes attributes = 0)
		{
			// Save the document into memory at first to avoid a torn file in the case of a serialization error.
			var ms = new MemoryStream();
			// Dispose cloned object to preserve keyframes identity in the original node. See Animator.Dispose().
			using (node = CreateCloneForSerialization(node)) {
				if (format == DocumentFormat.Scene) {
					var serializer = new HotSceneSerializer();
					TangerineYuzu.Instance.Value.WriteObject(path, ms, node, serializer);
				} else {
					TangerineYuzu.Instance.Value.WriteObject(path, ms, node, Serialization.Format.JSON);
				}
			}
			var fullPath = Project.Current.GetFullPath(path, GetFileExtension(format));

			FileMode fileModeForHiddenFile = File.Exists(fullPath) ? FileMode.Truncate : FileMode.Create;
			using (var fs = new FileStream(fullPath, fileModeForHiddenFile)) {
				var a = ms.ToArray();
				fs.Write(a, 0, a.Length);
			}
			var FileInfo = new FileInfo(fullPath);
			FileInfo.Attributes |= attributes;
		}

		public static Node CreateCloneForSerialization(Node node)
		{
			return Orange.Toolbox.CreateCloneForSerialization(node);
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
			if (!rowCache.TryGetValue(obj, out var row)) {
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
			}
			foreach (var child in node.Nodes) {
				Decorate(child);
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
