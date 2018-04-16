using Lime;
using Tangerine.Core;

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
				Document.SetCurrentFrameToNode(doc.PreviewAnimationBegin, doc.Container, CoreUserPreferences.Instance.AnimationMode);
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
