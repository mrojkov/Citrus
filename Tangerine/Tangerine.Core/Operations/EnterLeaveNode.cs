using Lime;
using System.Linq;

namespace Tangerine.Core.Operations
{
	public interface ISetContainer : IOperation { }

	public static class EnterNode
	{
		private class SetContainer : SetProperty, ISetContainer
		{
			public SetContainer(Node value) : base(Document.Current, nameof(Document.Container), value, false) { }
		}

		public static bool Perform(Node container, bool selectFirstNode = true)
		{
			if (!NodeCompositionValidator.CanHaveChildren(container.GetType())) {
				return false;
			}
			if (!string.IsNullOrEmpty(container.ContentsPath)) {
				OpenExternalScene(container.ContentsPath);
			} else {
				ChangeContainer(container, selectFirstNode);
				SetProperty.Perform(container, nameof(Node.TangerineFlags), container.TangerineFlags | TangerineFlags.DisplayContent, isChangingDocument: false);
			}
			return true;
		}

		private static void OpenExternalScene(string path)
		{
			var sceneNavigatedFrom = Document.Current.Path;
			var doc = Project.Current.OpenDocument(path);
			doc.SceneNavigatedFrom = sceneNavigatedFrom;
		}

		private static void ChangeContainer(Node container, bool selectFirstNode)
		{
			ClearRowSelection.Perform();
			var prevContainer = Document.Current.Container;
			DocumentHistory.Current.Perform(new SetContainer(container));
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
					var document = Project.Current.Documents.FirstOrDefault(i => i.Path == path);
					if (document == null) {
						document = Project.Current.OpenDocument(path);
					}
					document.MakeCurrent();
				}
			} else {
				var container = doc.Container;
				SetProperty.Perform(container, nameof(Node.TangerineFlags), container.TangerineFlags & ~TangerineFlags.DisplayContent, isChangingDocument: false);
				EnterNode.Perform(container.Parent, false);
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
