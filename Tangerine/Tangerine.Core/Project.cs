using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Lime;

namespace Tangerine.Core
{
	public class Project
	{
		readonly VersionedCollection<Document> documents = new VersionedCollection<Document>();
		public IReadOnlyVersionedCollection<Document> Documents => documents;

		private readonly object aggregateModifiedAssetsTaskTag = new object();
		private readonly HashSet<string> modifiedAssets = new HashSet<string>();
		private readonly List<Type> registeredNodeTypes = new List<Type>();
		private readonly List<Type> registeredComponentTypes = new List<Type>();

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
		public delegate void OpenFileOutsideProjectAttemptDelegate(string filePath);
		public static OpenFileOutsideProjectAttemptDelegate OpenFileOutsideProjectAttempt;
		public delegate void HandleMissingDocumentsDelegate(IEnumerable<Document> missingDocuments);
		public static HandleMissingDocumentsDelegate HandleMissingDocuments;
		public static TaskList Tasks { get; set; }
		public Dictionary<string, Widget> Overlays { get; } = new Dictionary<string, Widget>();
		public ProjectPreferences Preferences { get; private set; } = new ProjectPreferences();
		public ProjectUserPreferences UserPreferences { get; private set; } = new ProjectUserPreferences();
		public IReadOnlyList<Type> RegisteredNodeTypes => registeredNodeTypes;
		public IReadOnlyList<Type> RegisteredComponentTypes => registeredComponentTypes;

		public static event Action<Document> DocumentSaving;
		public static event Action<string> Opening;

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
			Opening?.Invoke(CitprojPath);
			if (Current != Null) {
				throw new InvalidOperationException();
			}
			Current = this;
			TangerineAssetBundle tangerineAssetBundle;
			AssetBundle.Current = tangerineAssetBundle = new TangerineAssetBundle(AssetsDirectory);
			if (!tangerineAssetBundle.IsActual()) {
				tangerineAssetBundle.CleanupBundle();
			}
			Preferences = new ProjectPreferences();
			Preferences.Initialize();
			if (File.Exists(UserprefsPath)) {
				try {
					UserPreferences = TangerineYuzu.Instance.Value.ReadObjectFromFile<ProjectUserPreferences>(UserprefsPath);
					foreach (var path in UserPreferences.Documents) {
						try {
							OpenDocument(path, delayLoad: true);
						} catch (System.Exception e) {
							Debug.Write($"Failed to open document '{path}': {e.Message}");
						}
					}
					HandleMissingDocuments(Documents.Where(d => !GetFullPath(d.Path, out string fullPath)));
					var currentDoc = documents.FirstOrDefault(d => d.Path == UserPreferences.CurrentDocument) ?? documents.FirstOrDefault();
					try {
						Document.SetCurrent(currentDoc);
					} catch (System.Exception e) {
						if (currentDoc != null) {
							CloseDocument(currentDoc);
						}
						throw;
					}
				} catch (System.Exception e) {
					Debug.Write($"Failed to load the project user preferences: {e}");
				}
			}
			fsWatcher = new Lime.FileSystemWatcher(AssetsDirectory, includeSubdirectories: true);
			fsWatcher.Changed += HandleFileSystemWatcherEvent;
			fsWatcher.Created += HandleFileSystemWatcherEvent;
			fsWatcher.Deleted += HandleFileSystemWatcherEvent;
			fsWatcher.Renamed += HandleFileSystemWatcherEvent;
			var overlaysPath = Path.Combine(Project.Current.AssetsDirectory, "Overlays");
			if (Directory.Exists(overlaysPath)) {
				var files = Directory.EnumerateFiles(overlaysPath)
					.Where(file => Path.GetExtension(file) == ".tan" || Path.GetExtension(file) == ".scene");
				foreach (var file in files) {
					Project.Current.Overlays.Add(Path.GetFileNameWithoutExtension(file), new Frame(file));
				}
			}

			registeredNodeTypes.AddRange(GetNodesTypesOrdered("Lime"));
			registeredComponentTypes.AddRange(GetComponentsTypes("Lime"));
			foreach (var type in Orange.PluginLoader.EnumerateTangerineExportedTypes()) {
				if (typeof(Node).IsAssignableFrom(type)) {
					registeredNodeTypes.Add(type);
				} else if (typeof(NodeComponent).IsAssignableFrom(type)) {
					registeredComponentTypes.Add(type);
				}
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
			UserPreferences.Documents.Clear();
			foreach (var doc in documents.ToList()) {
				UserPreferences.Documents.Add(doc.Path);
				if (!CloseDocument(doc)) {
					return false;
				}
			}
			try {
				TangerineYuzu.Instance.Value.WriteObjectToFile(UserprefsPath, UserPreferences, Serialization.Format.JSON);
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

		public Document NewDocument(DocumentFormat format = DocumentFormat.Scene, Type rootType = null)
		{
			var doc = new Document(format, rootType);
			documents.Add(doc);
			doc.MakeCurrent();
			return doc;
		}

		public string GetLocalDocumentPath(string path, bool pathIsAbsolute)
		{
			string localPath = path;
			if (pathIsAbsolute) {
				if (this == Null || !Current.TryGetAssetPath(path, out localPath)) {
					OpenFileOutsideProjectAttempt(path);
					return null;
				}
			}
			return AssetPath.CorrectSlashes(localPath);
		}

		public Document OpenDocument(string path, bool pathIsAbsolute = false, bool delayLoad = false)
		{
			var localPath = GetLocalDocumentPath(path, pathIsAbsolute);
			if (string.IsNullOrEmpty(localPath)) {
				return null;
			}
			var doc = Documents.FirstOrDefault(i => i.Path == localPath);
			if (doc == null) {
				var tmpFile = AutosaveProcessor.GetTemporaryFilePath(localPath);
				string systemPath;
				if (GetFullPath(tmpFile, out systemPath) && TempFileLoadConfirmation.Invoke(localPath)) {
					doc = new Document(tmpFile);
					doc.SaveAs(localPath);
				} else {
					doc = new Document(localPath, delayLoad);
				}
				if (systemPath != null) {
					File.Delete(systemPath);
				}
				documents.Add(doc);
			}
			if (!delayLoad) {
				doc.MakeCurrent();
			}
			AddRecentDocument(doc.Path);
			return doc;
		}

		public void AddRecentDocument(string path)
		{
			UserPreferences.RecentDocuments.Remove(path);
			UserPreferences.RecentDocuments.Insert(0, path);
			if (UserPreferences.RecentDocuments.Count > ProjectUserPreferences.MaxRecentDocuments)
				UserPreferences.RecentDocuments.RemoveAt(UserPreferences.RecentDocuments.Count - 1);
		}

		public bool CloseDocument(Document doc)
		{
			int currentIndex = documents.IndexOf(Document.Current);
			string systemPath;
			if (doc.Close()) {
				documents.Remove(doc);
				if (GetFullPath(AutosaveProcessor.GetTemporaryFilePath(doc.Path), out systemPath)) {
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

		public bool CloseAllDocuments()
		{
			// Checking and attempting to close modified documents
			for (var i = documents.Count() - 1; i >= 0; i--) {
				if (
					documents[i].IsModified && !CloseDocument(documents[i])) {
					return false;
				}
			}
			// Close others documents
			for (var i = documents.Count() - 1; i >= 0; i--) {
				if (!CloseDocument(documents[i])) {
					return false;
				}
			}
			return true;
		}

		public bool CloseAllDocumentsButThis(Document doc)
		{
			// Checking and attempting to close modified documents
			for (var i = documents.Count() - 1; i >= 0; i--) {
				if (
					documents[i].IsModified &&
					documents[i] != doc && !CloseDocument(documents[i])) {
					return false;
				}
			}
			// Close others documents
			for (var i = documents.Count() - 1; i >= 0; i--) {
				if (documents[i] != doc && !CloseDocument(documents[i])) {
					return false;
				}
			}
			return true;
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
			string modifiedAsset = null;
			if (path.EndsWith(".png")) {
				TexturePool.Instance.DiscardAllTextures();
			} else if (path == "#CookingRules.txt" || (path.EndsWith(".png.txt") && File.Exists(Path.ChangeExtension(path, null)))) {
				UpdateTextureParams();
				TexturePool.Instance.DiscardAllTextures();
			} else if (Document.AllowedFileTypes.Any(ext => path.EndsWith($".{ext}"))) {
				modifiedAsset = path;
			} else if (path.EndsWith(Model3DAttachment.FileExtension)) {
				var modelFileName = path.Remove(path.LastIndexOf(Model3DAttachment.FileExtension, StringComparison.InvariantCulture)) + ".fbx";
				if (File.Exists(modelFileName)) {
					modifiedAsset = modelFileName;
				}
			}
			if (!string.IsNullOrEmpty(modifiedAsset)) {
				RegisterModifiedAsset(modifiedAsset);
			}
		}

		public void ReloadModifiedDocuments()
		{
			HandleMissingDocuments(Documents.Where(d => !GetFullPath(d.Path, out string fullPath)));
			foreach (var doc in Documents.ToList()) {
				if (!doc.Loaded) {
					continue;
				}
				if (!File.Exists(doc.FullPath)) {
					throw new InvalidOperationException();
				} else if (doc.WasModifiedOutsideTangerine()) {
					if (DocumentReloadConfirmation(doc)) {
						ReloadDocument(doc);
					} else {
						doc.SetModificationTimeToNow();
					}
				} else {
					var requiredToRefreshExternalScenes = false;
					foreach (var descendant in doc.RootNodeUnwrapped.Descendants) {
						if (string.IsNullOrEmpty(descendant.ContentsPath)) {
							continue;
						}
						foreach (var modifiedAsset in modifiedAssets) {
							if (descendant.ContentsPath == modifiedAsset) {
								requiredToRefreshExternalScenes = true;
								break;
							}
						}
						if (requiredToRefreshExternalScenes) {
							doc.RefreshExternalScenes();
							break;
						}
					}
				}
			}
			modifiedAssets.Clear();
			Window.Current.Invalidate();
		}

		private void ReloadDocument(Document doc)
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
		}

		public void RevertDocument(Document doc)
		{
			ReloadDocument(doc);
		}

		private void RegisterModifiedAsset(string path)
		{
			string localPath;
			if (!TryGetAssetPath(path, out localPath)) {
				return;
			}
			localPath = AssetPath.CorrectSlashes(localPath);
			modifiedAssets.Add(localPath);

			Tasks.StopByTag(aggregateModifiedAssetsTaskTag);
			Tasks.Add(AggregateModifiedAssetsTask, aggregateModifiedAssetsTaskTag);
		}

		private IEnumerator<object> AggregateModifiedAssetsTask()
		{
			const float AggregationWaitTime = 0.5f;
			yield return AggregationWaitTime;
			yield return Task.WaitWhile(() => Application.Windows.All(window => !window.Active));
			ReloadModifiedDocuments();
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
						// buz: перед тем, как сохранять файл, надо убедиться, что там ещё не создан файл с точно такими же параметрами.
						// Потому что в зависимости от настроек системы/гита/Аллаха у сохраняемых файлов получаются разные line endings,
						// и если их постоянно перезаписывать, то они вечно будут показываться как изменённые в git, что жутко бесит,
						// и мешает работать.
						if (File.Exists(textureParamsPath)) {
							try {
								var existingParams = TangerineYuzu.Instance.Value.ReadObjectFromFile<TextureParams>(textureParamsPath);
								if (existingParams.Equals(textureParams)) {
									continue;
								}
							} catch (System.Exception) {
								// Подавляем исключения сериализации, потому что я не хочу, чтобы
								// этот костыль ещё и валил Танжерин по хз какому поводу.
							}
						}
						TangerineYuzu.Instance.Value.WriteObjectToFile(textureParamsPath, textureParams, Serialization.Format.JSON);
					} else if (File.Exists(textureParamsPath)) {
						File.Delete(textureParamsPath);
					}
				} else if (path.EndsWith(".texture") && !File.Exists(Path.Combine(AssetsDirectory, Path.ChangeExtension(path, ".png")))) {
					File.Delete(Path.Combine(AssetsDirectory, path));
				}
			}
		}

		public static void RaiseDocumetSaving(Document document)
		{
			DocumentSaving?.Invoke(document);
		}

		public static IEnumerable<Type> GetNodesTypesOrdered(string assemblyName)
		{
			var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == assemblyName);
			if (assembly == null) {
				return new List<Type>();
			}
			return assembly
				.GetTypes()
				.Where(t => typeof(Node).IsAssignableFrom(t) && t.IsDefined(typeof(TangerineRegisterNodeAttribute)))
				.ToDictionary(t => t, t => t.GetCustomAttributes(false).OfType<TangerineRegisterNodeAttribute>().First())
				.OrderBy(kv => kv.Value.Order)
				.Select(kv => kv.Key);
		}

		public static IEnumerable<Type> GetComponentsTypes(string assemblyName)
		{
			var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == assemblyName);
			if (assembly == null) {
				return new List<Type>();
			}
			return assembly
				.GetTypes()
				.Where(t => typeof(NodeComponent).IsAssignableFrom(t) && t.IsDefined(typeof(TangerineRegisterComponentAttribute)));
		}

		public string GetFullPath(string assetPath, string extension)
		{
			return Path.ChangeExtension(Path.Combine(AssetsDirectory, assetPath), extension);
		}

		public bool GetFullPath(string localPath, out string fullPath)
		{
			fullPath = null;
			foreach (var ext in Document.AllowedFileTypes) {
				fullPath = GetFullPath(localPath, ext);
				if (File.Exists(fullPath)) {
					return true;
				}
			}
			return false;
		}

		public static bool operator ==(Project lhs, Project rhs)
		{
			if (lhs is null) {
				lhs = Null;
			}
			if (rhs is null) {
				rhs = Null;
			}
			return Equals(rhs, lhs);
		}

		public static bool operator !=(Project lhs, Project rhs) => !(lhs == rhs);
	}
}
