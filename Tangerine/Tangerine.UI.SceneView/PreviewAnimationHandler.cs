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
			Document.Current.TogglePreviewAnimation(CoreUserPreferences.Instance.AnimationMode, triggerMarkersBeforeCurrentFrame);
		}
	}
}
