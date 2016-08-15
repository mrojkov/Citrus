using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public static class EnterNode
	{
		public static void Perform(Node container)
		{
			if (container.ContentsPath != null) {
				OpenExternalScene(container.ContentsPath);
			} else {
				ChangeContainer(container);
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

		public static void ChangeContainer(Node container)
		{
			var timeline = Timeline.Instance;
			var prevContainer = Timeline.Instance.Container;
			DelegateOperation.Perform(
				() => {
					timeline.Container = container;
					timeline.EnsureColumnVisible(Document.Current.AnimationFrame);
				},
				() => {
					timeline.Container = prevContainer;
					timeline.EnsureColumnVisible(Document.Current.AnimationFrame);
				}
			);
			ClearRowSelection.Perform();
			if (container.Nodes.Count > 0) {
				var r = timeline.GetCachedRow(container.Nodes[0].EditorState().Uid);
				SelectRow.Perform(r);
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
				EnterNode.Perform(doc.Container.Parent);
			}
		}
	}
}