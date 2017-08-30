using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	public class PreviewAnimationHandler : DocumentCommandHandler
	{
		public override void Execute()
		{
			var doc = Core.Document.Current;
			if (doc.PreviewAnimation) {
				doc.PreviewAnimation = false;
				StopAnimationRecursive(doc.PreviewAnimationContainer);
				doc.PreviewAnimationContainer.AnimationFrame = doc.PreviewAnimationBegin;
				doc.Container.Update(0);
			} else {
				doc.PreviewAnimation = true;
				doc.Container.IsRunning = doc.PreviewAnimation;
				doc.PreviewAnimationBegin = doc.Container.AnimationFrame;
				doc.PreviewAnimationContainer = doc.Container;
			}
			Application.InvalidateWindows();
		}

		void StopAnimationRecursive(Node node)
		{
			node.IsRunning = false;
			node.AnimationFrame = 0;
			foreach (var child in node.Nodes) {
				StopAnimationRecursive(child);
			}
		}
	}
}