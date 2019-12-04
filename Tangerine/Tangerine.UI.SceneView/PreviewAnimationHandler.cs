using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class PreviewAnimationHandler : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			Document.Current.TogglePreviewAnimation();
		}
	}
}
