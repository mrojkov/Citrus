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

		public static readonly Project Null = new Project();
		public static Project Current { get; private set; } = Null;

		public readonly string CitprojPath;
		public readonly string UserprefsPath;
		public readonly string AssetsDirectory;

		private Project() { }

		public Project(string citprojPath)
		{
			CitprojPath = citprojPath;
			UserprefsPath = Path.ChangeExtension(citprojPath, ".userprefs");
			AssetsDirectory = Path.Combine(Path.GetDirectoryName(CitprojPath), "Data");
		}

		public void Open()
		{
			if (Current != Null) {
				throw new InvalidOperationException();
			}
			Current = this;
			AssetsBundle.Instance = new UnpackedAssetsBundle(AssetsDirectory);
			if (File.Exists(UserprefsPath)) {
				try {
					var userprefs = Serialization.ReadObjectFromFile<Userprefs>(UserprefsPath);
					foreach (var path in userprefs.Documents) {
						if (AssetsBundle.Instance.FileExists(path)) {
							try {
								OpenDocument(path);
							} catch (System.Exception e) {
								Debug.Write($"Failed to open document '{path}': {e.Message}");
							}
						}
					}
					var currentDoc = documents.FirstOrDefault(d => d.Path == userprefs.CurrentDocument) ?? documents.FirstOrDefault();
					Document.SetCurrent(currentDoc);
				} catch (System.Exception e) {
					Debug.Write($"Failed to load the project user preferences: {e}");
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
			Serialization.WriteObjectToFile(UserprefsPath, userprefs, Serialization.Format.JSON);
			AssetsBundle.Instance = null;
			Current = Null;
			return true;
		}

		public bool AbsoluteToAssetPath(string absolutePath, out string assetPath)
		{
			assetPath = null;
			if (absolutePath.StartsWith(AssetsDirectory)) {
				assetPath = absolutePath.Substring(AssetsDirectory.Length + 1);
				return true;
			}
			return false;
		}

		public string AssetToAbsolutePath(string assetPath) => Path.Combine(AssetsDirectory, assetPath);

		public Document NewDocument()
		{
			var doc = new Document();
			documents.Add(doc);
			doc.MakeCurrent();
			return doc;
		}
			
		public Document OpenDocument(string path, bool selectFirstNode = true)
		{
			var doc = Documents.FirstOrDefault(i => i.Path == path);
			if (doc == null) {
				doc = new Document(path);
				documents.Add(doc);
			}
			doc.MakeCurrent();
			if (selectFirstNode) {
				Operations.Dummy.Perform();
				if (doc.Rows.Count > 0) {
					Operations.SelectRow.Perform(doc.Rows[0]);
				}
			}
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

		public class Userprefs
		{
			[YuzuMember]
			public readonly List<string> Documents = new List<string>();

			[YuzuMember]
			public string CurrentDocument;
		}
	}
}
