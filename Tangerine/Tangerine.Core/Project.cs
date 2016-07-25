using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ProtoBuf;
using Lime;
using Tangerine.Core;

namespace Tangerine.Core
{
	public class Project
	{
		public static readonly Project Null = new Project();
		public readonly ObservableCollection<Document> Documents = new ObservableCollection<Document>();
		public readonly string CitprojPath;
		public readonly string UserprefsPath;

		public static Project Current { get; private set; } = Null;

		private Project() { }

		public Project(string citprojPath)
		{
			CitprojPath = citprojPath;
			Documents.CollectionChanged += Documents_CollectionChanged;
			UserprefsPath = Path.ChangeExtension(citprojPath, ".userprefs");
			if (File.Exists(UserprefsPath)) {
				try {
					var userprefs = Serialization.ReadObjectFromFile<Userprefs>(UserprefsPath);
					foreach (var path in userprefs.Documents) {
						OpenDocument(path);
					}
					var currentDoc = Documents.FirstOrDefault(d => d.Path == userprefs.CurrentDocument);
					Document.SetCurrent(currentDoc);
				} catch (System.Exception e) {
					Debug.Write($"Failed to load the project user preferences: {e}");
				}
			}
		}

		void Documents_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null) {
				foreach (var d in e.NewItems.OfType<Document>()) {
					d.Closed += RemoveDocument;
				}
			}
			if (e.OldItems != null) {
				foreach (var d in e.OldItems.OfType<Document>()) {
					d.Closed -= RemoveDocument;
				}
			}
		}

		public bool Close()
		{
			var userprefs = new Userprefs();
			if (Document.Current != null) {
				userprefs.CurrentDocument = Document.Current.Path;
			}
			foreach (var doc in Documents) {
				if (!doc.Close()) {
					return false;
				}
				userprefs.Documents.Add(doc.Path);
			}
			Serialization.WriteObjectToFile(UserprefsPath, userprefs);
			AssetsBundle.Instance = null;
			return true;
		}

		public ScopedAssetsBundle SetsAssetsBundle()
		{
			var assetsPath = Path.Combine(Path.GetDirectoryName(CitprojPath), "Data");
			return new ScopedAssetsBundle(new UnpackedAssetsBundle(assetsPath));
		}

		public static void SetCurrent(Project proj)
		{
			Current = proj;
			if (proj != null) {
				proj.SetsAssetsBundle();
			} else {
				AssetsBundle.Instance = null;
			}
		}

		public Document OpenDocument(string path)
		{
			var doc = new Document(path);
			Documents.Add(doc);
			doc.MakeCurrent();
			return doc;
		}
			
		void RemoveDocument(Document doc)
		{
			int currentIndex = Documents.IndexOf(Document.Current);
			Documents.Remove(doc);
			if (doc == Document.Current) {
				if (Documents.Count > 1) {
					Documents[currentIndex.Min(Documents.Count - 1)].MakeCurrent();
				} else {
					Document.SetCurrent(null);
				}
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

	public struct ScopedAssetsBundle : IDisposable
	{
		AssetsBundle oldBundle;

		public ScopedAssetsBundle(AssetsBundle bundle)
		{
			oldBundle = AssetsBundle.Initialized ? AssetsBundle.Instance : null;
			AssetsBundle.Instance = bundle;
		}

		public void Dispose()
		{
			AssetsBundle.Instance = oldBundle;
		}
	}
}