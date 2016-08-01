using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ProtoBuf;
using Lime;

namespace Tangerine.Core
{
	public class Project
	{
		readonly VersionedCollection<Document> documents = new VersionedCollection<Document>();
		public IReadOnlyVersionedCollection<Document> Documents => documents;

		public static readonly Project Null = new Project();
		public readonly string CitprojPath;
		public readonly string UserprefsPath;

		public static Project Current { get; private set; } = Null;

		private Project() { }

		public Project(string citprojPath)
		{
			if (Current != Null) {
				throw new InvalidOperationException();
			}
			CitprojPath = citprojPath;
			UserprefsPath = Path.ChangeExtension(citprojPath, ".userprefs");
			if (File.Exists(UserprefsPath)) {
				try {
					var userprefs = Serialization.ReadObjectFromFile<Userprefs>(UserprefsPath);
					foreach (var path in userprefs.Documents) {
						OpenDocument(path);
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
			Serialization.WriteObjectToFile(UserprefsPath, userprefs);
			AssetsBundle.Instance = null;
			return true;
		}

		public AssetsBundle GetAssetsBundle()
		{
			if (this == Null) {
				return null;
			}
			var assetsPath = Path.Combine(Path.GetDirectoryName(CitprojPath), "Data");
			return new UnpackedAssetsBundle(assetsPath);
		}

		public static void SetCurrent(Project proj)
		{
			Current = proj;
			AssetsBundle.Instance = proj.GetAssetsBundle();
		}

		public Document OpenDocument(string path)
		{
			var doc = new Document(path);
			documents.Add(doc);
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
			
		[ProtoContract]
		public class Userprefs
		{
			[ProtoMember(1)]
			public readonly List<string> Documents = new List<string>();

			[ProtoMember(2)]
			public string CurrentDocument;
		}
	}
}