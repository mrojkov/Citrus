using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Yuzu;
using Lime;

namespace Tangerine.Core
{
	public class Project
	{
		readonly VersionedCollection<Document> documents = new VersionedCollection<Document>();
		public IReadOnlyVersionedCollection<Document> Documents => documents;

		private Lime.FileSystemWatcher fsWatcher;

		public static readonly Project Null = new Project();
		public static Project Current { get; private set; } = Null;

		public readonly string CitprojPath;
		public readonly string UserprefsPath;
		public readonly string AssetsDirectory;

		public delegate bool DocumentReloadConfirmationDelegate(Document document);
		public static DocumentReloadConfirmationDelegate DocumentReloadConfirmation;

		private Project() { }

		public Project(string citprojPath)
		{
			CitprojPath = citprojPath;
			UserprefsPath = Path.ChangeExtension(citprojPath, ".userprefs");
			AssetsDirectory = Path.Combine(Path.GetDirectoryName(CitprojPath), "Data");
			Orange.The.Workspace.Open(citprojPath);
		}

		public void Open()
		{
			if (Current != Null) {
				throw new InvalidOperationException();
			}
			Current = this;
			AssetBundle.Instance = new UnpackedAssetBundle(AssetsDirectory);
			if (File.Exists(UserprefsPath)) {
				try {
					var userprefs = Serialization.ReadObjectFromFile<Userprefs>(UserprefsPath);
					foreach (var path in userprefs.Documents) {
						try {
							OpenDocument(path);
						} catch (System.Exception e) {
							Debug.Write($"Failed to open document '{path}': {e.Message}");
						}
					}
					var currentDoc = documents.FirstOrDefault(d => d.Path == userprefs.CurrentDocument) ?? documents.FirstOrDefault();
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
			var userprefs = new Userprefs();
			if (Document.Current != null) {
				userprefs.CurrentDocument = Document.Current.Path;
			}
			foreach (var doc in documents.ToList()) {
				if (!CloseDocument(doc)) {
					return false;
				}
				userprefs.Documents.Add(doc.Path);
			}
			try {
				Serialization.WriteObjectToFile(UserprefsPath, userprefs, Serialization.Format.JSON);
			} catch (System.Exception) { }
			AssetBundle.Instance = null;
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

		public Document OpenDocument(string path)
		{
			var doc = Documents.FirstOrDefault(i => i.Path == path);
			if (doc == null) {
				doc = new Document(path);
				documents.Add(doc);
			}
			doc.MakeCurrent();
			return doc;
		}

		public bool CloseDocument(Document doc)
		{
			int currentIndex = documents.IndexOf(Document.Current);
			if (doc.Close()) {
				documents.Remove(doc);
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
			}
			ReloadModifiedDocuments();
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

		public class Userprefs
		{
			[YuzuMember]
			public readonly List<string> Documents = new List<string>();

			[YuzuMember]
			public string CurrentDocument;
		}
	}
}
