using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class PreviewAnimationHandler : DocumentCommandHandler
	{

		private readonly bool triggerMarkersBeforeCurrentFrame;

		public PreviewAnimationHandler(bool triggerMarkersBeforeCurrentFrame)
		{
			this.triggerMarkersBeforeCurrentFrame = triggerMarkersBeforeCurrentFrame;
		}

		public override void Execute()
		{
			var doc = Document.Current;
			if (doc.PreviewAnimation) {
				doc.PreviewAnimation = false;
				StopAnimationRecursive(doc.PreviewAnimationContainer);
				Document.SetCurrentFrameToNode(
					doc.PreviewAnimationBegin, doc.Container, CoreUserPreferences.Instance.AnimationMode
				);
				AudioSystem.StopAll();
			} else {
				int savedAnimationFrame = doc.Container.AnimationFrame;
				doc.PreviewAnimation = true;
				if (triggerMarkersBeforeCurrentFrame) {
					Document.SetCurrentFrameToNode(0, doc.Container, true);
				}
				doc.Container.IsRunning = doc.PreviewAnimation;
				if (triggerMarkersBeforeCurrentFrame) {
					Document.FastForwardToFrame(doc.Container, savedAnimationFrame);
				}
				doc.PreviewAnimationBegin = savedAnimationFrame;
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
