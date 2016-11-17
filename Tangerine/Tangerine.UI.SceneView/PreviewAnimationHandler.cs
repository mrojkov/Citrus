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
			if (doc.Container.IsRunning) {
				doc.Container.AnimationFrame = doc.PreviewAnimationBegin;
			} else {
				doc.PreviewAnimationBegin = doc.Container.AnimationFrame;
			}
			doc.PreviewAnimation = !doc.PreviewAnimation;
			Application.InvalidateWindows();
		}
	}
}