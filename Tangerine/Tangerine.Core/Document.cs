using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;

namespace Tangerine.Core
{
	public interface ISelectedNodesProvider
	{
		IEnumerable<Node> Get();
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
		
		public static Document Current { get; private set; }

		public string Path { get; private set; }
		public readonly DocumentHistory History;
		public bool IsModified => History.IsDocumentModified;

		public static Func<Document, CloseAction> Closing;

		public Node RootNode { get; private set; }
		public Node Container { get; set; }
		public IEnumerable<Node> SelectedNodes => SelectedNodesProvider.Get();

		private bool viewsCreated;
		public readonly List<IDocumentView> Views = new List<IDocumentView>();

		public int AnimationFrame
		{
			get { return Container.AnimationFrame; }
			set { Container.AnimationFrame = value; }
		}

		public string AnimationId { get; set; }
		public ISelectedNodesProvider SelectedNodesProvider { get; set; }

		public Document()
		{
			History = new DocumentHistory();
		}

		public Document(string path) : this()
		{
			Path = path;
			using (Theme.Push(DefaultTheme.Instance)) {
				RootNode = Serialization.ReadObject<Node>(path);
			}
			Container = RootNode;
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
			if (!IsModified) {
				return true;
			}
			if (Closing != null) {
				var r = Closing(this);
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
		}
	}
}
