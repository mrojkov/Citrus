using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Yuzu;
using Lime;

namespace Tangerine.Core
{
	public class Project
	{
		readonly VersionedCollection<Document> documents = new VersionedCollection<Document>();
		public IReadOnlyVersionedCollection<Document> Documents => documents;

		private readonly object aggregateModifiedAssetsTaskTag = new object();
		private readonly HashSet<string> modifiedAssets = new HashSet<string>();

		private volatile bool isCookingModifiedAssets;
		private Lime.FileSystemWatcher fsWatcher;

		public static readonly Project Null = new Project();
		public static Project Current { get; private set; } = Null;

		public readonly string CitprojPath;
		public readonly string UserprefsPath;
		public readonly string AssetsDirectory;

		public delegate bool DocumentReloadConfirmationDelegate(Document document);
		public static DocumentReloadConfirmationDelegate DocumentReloadConfirmation;
		public delegate bool TempFileLoadConfirmationDelegate(string path);
		public static TempFileLoadConfirmationDelegate TempFileLoadConfirmation;
		public delegate void CookingOfModifiedAssetsStartedDelegate();
		public static CookingOfModifiedAssetsStartedDelegate CookingOfModifiedAssetsStarted;
		public delegate void CookingOfModifiedAssetsEndedDelegate();
		public static CookingOfModifiedAssetsEndedDelegate CookingOfModifiedAssetsEnded;
		public delegate void OpenFileOutsideProjectAttemptDelegate(string filePath);
		public static OpenFileOutsideProjectAttemptDelegate OpenFileOutsideProjectAttempt;
		public static volatile string CookingOfModifiedAssetsStatus;
		public static TaskList Tasks { get; set; }
		public Dictionary<string, Widget> Overlays { get; } = new Dictionary<string, Widget>();
		public ProjectUserPreferences UserPreferences { get; private set; } = new ProjectUserPreferences();

		private Project() { }

		public Project(string citprojPath)
		{
			CitprojPath = citprojPath;
			UserprefsPath = Path.ChangeExtension(citprojPath, ".userprefs");
			AssetsDirectory = Path.Combine(Path.GetDirectoryName(CitprojPath), "Data");
			if (!Directory.Exists(AssetsDirectory)) {
				throw new InvalidOperationException($"Assets directory {AssetsDirectory} doesn't exist.");
			}
			Orange.The.Workspace.Open(citprojPath);
			UpdateTextureParams();
		}

		public void Open()
		{
			if (Current != Null) {
				throw new InvalidOperationException();
			}
			Current = this;
			AssetBundle.Current = new UnpackedAssetBundle(AssetsDirectory);
			if (File.Exists(UserprefsPath)) {
				try {
					UserPreferences = Serialization.ReadObjectFromFile<ProjectUserPreferences>(UserprefsPath);
					foreach (var path in UserPreferences.Documents) {
						try {
							OpenDocument(path);
						} catch (System.Exception e) {
							Debug.Write($"Failed to open document '{path}': {e.Message}");
						}
					}
					var currentDoc = documents.FirstOrDefault(d => d.Path == UserPreferences.CurrentDocument) ?? documents.FirstOrDefault();
					Document.SetCurrent(currentDoc);
				} catch (System.Exception e) {
					Debug.Write($"Failed to load the project user preferences: {e}");
				}
			}
			fsWatcher = new Lime.FileSystemWatcher(AssetsDirectory, includeSubdirectories: true);
			fsWatcher.Changed += HandleFileSystemWatcherEvent;
			fsWatcher.Created += HandleFileSystemWatcherEvent;
			fsWatcher.Deleted += HandleFileSystemWatcherEvent;
			fsWatcher.Renamed += HandleFileSystemWatcherEvent;
			try {
				var files = Directory.EnumerateFiles(Path.Combine(Project.Current.AssetsDirectory, "Overlays"))
					.Where(file => Path.GetExtension(file) == ".tan" || Path.GetExtension(file) == ".scene");
				foreach (var file in files) {
					Project.Current.Overlays.Add(Path.GetFileNameWithoutExtension(file), new Frame(file));
				}
			} catch (DirectoryNotFoundException e) {
				Debug.Write("Failed to load Overlays: directory not found");
			}
		}

		public bool Close()
		{
			if (Current != this) {
				throw new InvalidOperationException();
			}
			if (Current == Null) {
				return true;
			}
			fsWatcher?.Dispose();
			fsWatcher = null;
			foreach (var doc in documents.ToList()) {
				if (!CloseDocument(doc)) {
					return false;
				}
			}
			try {
				Serialization.WriteObjectToFile(UserprefsPath, UserPreferences, Serialization.Format.JSON);
			} catch (System.Exception) { }
			AssetBundle.Current = null;
			Current = Null;
			return true;
		}

		public bool TryGetAssetPath(string systemPath, out string assetPath)
		{
			assetPath = null;
			var p = Path.ChangeExtension(systemPath, null);
			if (p.StartsWith(AssetsDirectory)) {
				assetPath = p.Substring(AssetsDirectory.Length + 1);
				return true;
			}
			return false;
		}

		public string GetSystemPath(string assetPath, string extension)
		{
			return Path.ChangeExtension(Path.Combine(AssetsDirectory, assetPath), extension);
		}

		public string GetSystemDirectory(string assetPath)
		{
			return Path.GetDirectoryName(GetSystemPath(assetPath, null));
		}

		public Document NewDocument()
		{
			var doc = new Document();
			documents.Add(doc);
			doc.MakeCurrent();
			return doc;
		}

		public Document OpenDocument(string path, bool pathIsGlobal = false)
		{
			string localPath = path;
			if (pathIsGlobal) {
				if (!Current.TryGetAssetPath(path, out localPath)) {
					OpenFileOutsideProjectAttempt(path);
					return null;
				}
			}

			var doc = Documents.FirstOrDefault(i => i.Path == localPath);
			if (doc == null) {
				var tmpFile = AutosaveProcessor.GetTemporalFilePath(localPath);
				string systemPath;
				if (GetSystemPath(tmpFile, out systemPath) && TempFileLoadConfirmation.Invoke(localPath)) {
					doc = new Document(tmpFile);
					doc.SaveAs(localPath);
				} else {
					doc = new Document(localPath);
				}
				if (systemPath != null) {
					File.Delete(systemPath);
				}
				documents.Add(doc);
				if (!UserPreferences.Documents.Contains(doc.Path)) {
					UserPreferences.Documents.Add(doc.Path);
				}
			}
			doc.MakeCurrent();
			return doc;
		}

		public bool GetSystemPath(string localPath, out string systemPath)
		{
			systemPath = null;
			foreach (var ext in Document.AllowedFileTypes) {
				systemPath = GetSystemPath(localPath, ext);
				if (File.Exists(systemPath)) {
					return true;
				}
			}
			return false;
		}

		public bool CloseDocument(Document doc)
		{
			int currentIndex = documents.IndexOf(Document.Current);
			string systemPath;
			if (doc.Close()) {
				documents.Remove(doc);
				UserPreferences.Documents.Remove(doc.Path);
				if (GetSystemPath(AutosaveProcessor.GetTemporalFilePath(doc.Path), out systemPath)) {
					File.Delete(systemPath);
				}
				if (doc == Document.Current) {
					if (documents.Count > 0) {
						documents[currentIndex.Min(Documents.Count - 1)].MakeCurrent();
					} else {
						Document.SetCurrent(null);
					}
				}
				return true;
			}
			return false;
		}

		public void NextDocument()
		{
			AdvanceDocument(1);
		}

		public void PreviousDocument()
		{
			AdvanceDocument(-1);
		}

		private void AdvanceDocument(int direction)
		{
			if (documents.Count > 0) {
				int currentIndex = documents.IndexOf(Document.Current);
				documents[(currentIndex + direction).Wrap(0, Documents.Count - 1)].MakeCurrent();
			}
		}

		public void HandleFileSystemWatcherEvent(string path)
		{
			if (path.EndsWith(".png")) {
				TexturePool.Instance.DiscardAllTextures();
			} else if (path == "#CookingRules.txt" || (path.EndsWith(".png.txt") && File.Exists(Path.ChangeExtension(path, null)))) {
				UpdateTextureParams();
				TexturePool.Instance.DiscardAllTextures();
			} else if (path.EndsWith(".fbx")) {
				RegisterModifiedAsset(path);
			} else if (path.EndsWith(Model3DAttachment.FileExtension)) {
				var modelFileName = path.Remove(path.LastIndexOf(Model3DAttachment.FileExtension, StringComparison.InvariantCulture)) + ".fbx";
				if (File.Exists(modelFileName)) {
					RegisterModifiedAsset(modelFileName);
				}
			}
			ReloadModifiedDocuments();
			Window.Current.Invalidate();
		}

		public void ReloadModifiedDocuments()
		{
			foreach (var doc in Documents.ToList()) {
				if (doc.WasModifiedOutsideTangerine()) {
					if (DocumentReloadConfirmation(doc)) {
						ReloadDocument(doc);
					} else {
						doc.SetModificationTimeToNow();
					}
				}
			}
		}

		void ReloadDocument(Document doc)
		{
			int index = documents.IndexOf(doc);
			documents.Remove(doc);
			var newDoc = new Document(doc.Path);
			documents.Insert(index, newDoc);
			var savedCurrent = Document.Current == doc ? null : Document.Current;
			newDoc.MakeCurrent();
			savedCurrent?.MakeCurrent();
		}

		public void ReorderDocument(Document doc, int toIndex)
		{
			int previousIndex = documents.IndexOf(doc);
			if (previousIndex < 0) return;
			documents.Remove(doc);
			documents.Insert(toIndex, doc);
			UserPreferences.Documents.Remove(doc.Path);
			UserPreferences.Documents.Insert(toIndex, doc.Path);
		}

		public void RevertDocument(Document doc)
		{
			ReloadDocument(doc);
		}

		private void RegisterModifiedAsset(string path)
		{
			path = path.Remove(0, AssetsDirectory.Length + 1);
			path = Orange.CsprojSynchronization.ToUnixSlashes(path);
			lock (modifiedAssets) {
				modifiedAssets.Add(path);
			}

			Tasks.StopByTag(aggregateModifiedAssetsTaskTag);
			Tasks.Add(AggregateModifiedAssetsTask, aggregateModifiedAssetsTaskTag);
		}

		private IEnumerator<object> AggregateModifiedAssetsTask()
		{
			const float AggregationWaitTime = 2f;
			yield return AggregationWaitTime;
			yield return Task.WaitWhile(() => isCookingModifiedAssets);

			isCookingModifiedAssets = true;
			CookingOfModifiedAssetsAsync();
			CookingOfModifiedAssetsStarted?.Invoke();
		}

		private async void CookingOfModifiedAssetsAsync()
		{
			List<string> assets;
			lock (modifiedAssets) {
				assets = modifiedAssets.ToList();
				modifiedAssets.Clear();
			}

			try {
				await System.Threading.Tasks.Task.Run(() => RecookAssets(assets));
			} catch (System.Exception e) {
				Console.WriteLine(e);
			}
			AssetBundle.Current = new UnpackedAssetBundle(AssetsDirectory);

			foreach (var document in Documents) {
				document.RefreshExternalScenes();
			}

			CookingOfModifiedAssetsStatus = null;
			CookingOfModifiedAssetsEnded?.Invoke();

			isCookingModifiedAssets = false;
		}

		private static void RecookAssets(IEnumerable<string> assets)
		{
			foreach (var asset in assets) {
				CookingOfModifiedAssetsStatus = asset;
				Orange.AssetCooker.CookCustomAssets(Orange.The.Workspace.ActivePlatform, new List<string> { asset });
			}
		}

		private void UpdateTextureParams()
		{
			var rules = Orange.CookingRulesBuilder.Build(Orange.The.Workspace.AssetFiles, null);
			foreach (var kv in rules) {
				var path = kv.Key;
				var rule = kv.Value;
				if (path.EndsWith(".png")) {
					var textureParamsPath = Path.Combine(Orange.The.Workspace.AssetsDirectory, Path.ChangeExtension(path, ".texture"));
					if (!Orange.AssetCooker.AreTextureParamsDefault(rule)) {
						var textureParams = new TextureParams {
							WrapMode = rule.WrapMode,
							MinFilter = rule.MinFilter,
							MagFilter = rule.MagFilter,
						};
						Serialization.WriteObjectToFile(textureParamsPath, textureParams, Serialization.Format.JSON);
					} else if (File.Exists(textureParamsPath)) {
						File.Delete(textureParamsPath);
					}
				} else if (path.EndsWith(".texture") && !File.Exists(Path.Combine(AssetsDirectory, Path.ChangeExtension(path, ".png")))) {
					File.Delete(Path.Combine(AssetsDirectory, path));
				}
			}
		}
	}
}
