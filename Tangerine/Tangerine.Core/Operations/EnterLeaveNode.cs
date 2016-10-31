using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.Core.Operations
{
	public interface ISetContainer : IOperation { }

	public static class EnterNode
	{
		class SetContainer : SetProperty, ISetContainer
		{
			public SetContainer(Node value) : base(Document.Current, nameof(Document.Container), value) { }
			public override bool IsChangingDocument => false;
		}

		public static bool Perform(Node container, bool selectFirstNode = true)
		{
			if (!ClassAttributes<TangerineClassAttribute>.Get(container.GetType())?.AllowChildren ?? true) {
				return false;
			}
			if (container.ContentsPath != null) {
				OpenExternalScene(container.ContentsPath);
			} else {
				ChangeContainer(container, selectFirstNode);
			}
			return true;
		}

		static void OpenExternalScene(string path)
		{
			path = System.IO.Path.ChangeExtension(path, Document.SceneFileExtension);
			var sceneNavigatedFrom = Document.Current.Path;
			var doc = Project.Current.OpenDocument(path);
			doc.SceneNavigatedFrom = sceneNavigatedFrom;
		}

		static void ChangeContainer(Node container, bool selectFirstNode)
		{
			ClearRowSelection.Perform();
			var prevContainer = Document.Current.Container;
			Document.Current.History.Perform(new SetContainer(container));
			if (selectFirstNode && container.Nodes.Count > 0) {
				SelectNode.Perform(container.Nodes[0]);
			}
		}
	}

	public static class LeaveNode
	{
		public static void Perform()
		{
			var doc = Document.Current;
			if (doc.Container == doc.RootNode) {
				var path = doc.SceneNavigatedFrom;
				if (path != null) {
					path = System.IO.Path.ChangeExtension(path, Document.SceneFileExtension);
					Project.Current.Documents.FirstOrDefault(i => i.Path == path)?.MakeCurrent();
				}
			} else {
				var container = doc.Container;
				EnterNode.Perform(doc.Container.Parent, false);
				SelectNode.Perform(container, true);
			}
		}

		public static bool IsAllowed()
		{
			var doc = Document.Current;
			return doc.Container != doc.RootNode || doc.SceneNavigatedFrom != null;
		}
	}
}