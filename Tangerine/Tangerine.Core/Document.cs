using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;

namespace Tangerine.Core
{
	public interface ISelectedObjectsProvider
	{
		IEnumerable<object> Get();
	}

	public interface IDocumentView
	{
		void Detach();
		void Attach();
	}

	public sealed class Document
	{
		public enum CloseAction
		{
			Cancel,
			SaveChanges,
			DiscardChanges
		}

		public static Action ViewsBuilder;
		
		public static readonly Document Null = new Document { ReadOnly = true };
		public static Document Current { get; private set; }

		public string Path { get; private set; }
		public readonly DocumentHistory History;
		public bool ReadOnly { get; private set; }
		public bool IsModified => History.UndoEnabled;

		public event Func<CloseAction> Closing;
		public event Action<Document> Closed;
		
		public Node RootNode { get; private set; }
		public Node Container { get; set; }
		public IEnumerable<object> SelectedObjects => SelectedObjectsProvider.Get();

		private bool viewsCreated;
		public readonly List<IDocumentView> Views = new List<IDocumentView>();

		public int AnimationFrame
		{
			get { return Container.AnimationFrame; }
			set { Container.AnimationFrame = value; }
		}

		public string AnimationId { get; set; }
		public ISelectedObjectsProvider SelectedObjectsProvider { get; set; }

		public Document()
		{
			History = new DocumentHistory();
		}

		public Document(string path) : this()
		{
			Path = path;
			ReadOnly = IsFileReadonly(path);
			RootNode = new Orange.HotSceneImporter(path).ParseNode();
			Container = RootNode;
		}

		private static bool IsFileReadonly(string path)
		{
			return (File.GetAttributes(path) & FileAttributes.ReadOnly) != 0;
		}

		public void MakeCurrent()
		{
			SetCurrent(this);
		}

		public static void SetCurrent(Document doc)
		{
			if (Current != null) {
				Current.DetachViews();
			}
			Current = doc;
			if (doc == null) {
			} else {
				doc.AttachViews();
			}
		}

		void AttachViews()
		{
			if (!viewsCreated) {
				ViewsBuilder();
				viewsCreated = true;
			}
			foreach (var i in Current.Views) {
				i.Attach();
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
			if (Closing != null) {
				var a = Closing();
				if (a == CloseAction.Cancel) {
					return false;
				}
				if (a == CloseAction.SaveChanges) {
					Save();
				}
			}
			if (Closed != null) {
				Closed(this);
			}
			return true;
		}

		public void Save()
		{
		}
	}
}
