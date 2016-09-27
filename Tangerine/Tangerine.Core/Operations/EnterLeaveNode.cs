using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.Core.Operations
{
	public static class EnterNode
	{
		public static void Perform(Node container, bool selectFirstNode = true)
		{
			if (container.ContentsPath != null) {
				OpenExternalScene(container.ContentsPath);
			} else {
				ChangeContainer(container, selectFirstNode);
			}
		}

		static void OpenExternalScene(string path)
		{
			path = System.IO.Path.ChangeExtension(path, Document.SceneFileExtension);
			var doc = Project.Current.Documents.FirstOrDefault(i => i.Path == path);
			var sceneNavigatedFrom = Document.Current.Path;
			if (doc != null) {
				doc.MakeCurrent();
			} else {
				doc = Project.Current.OpenDocument(path);
			}
			doc.SceneNavigatedFrom = sceneNavigatedFrom;
		}

		public static void ChangeContainer(Node container, bool selectFirstNode)
		{
			var prevContainer = Document.Current.Container;
			DelegateOperation.Perform(() => SetContainer(container), () => SetContainer(prevContainer));
			ClearRowSelection.Perform();
			if (selectFirstNode && container.Nodes.Count > 0) {
				SelectNode.Perform(container.Nodes[0]);
			}
		}

		static void SetContainer(Node container)
		{
			Document.Current.Container = container;
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
				EnterNode.Perform(doc.Container.Parent);
			}
		}
	}
}