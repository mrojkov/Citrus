using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	public class PreviewAnimationProcessor : Core.ITaskProvider
	{
		readonly WidgetInput input;

		public PreviewAnimationProcessor(WidgetInput input)
		{
			this.input = input;
		}

		public IEnumerator<object> Task()
		{
			int savedFrame = 0;
			while (true) {
				yield return null;
				var doc = Core.Document.Current;
				if (doc == null)
					continue;
				if (input.ConsumeKeyPress(KeyBindings.SceneViewKeys.PreviewAnimation)) {
					if (doc.Container.IsRunning) {
						doc.Container.AnimationFrame = savedFrame;
					} else {
						savedFrame = doc.Container.AnimationFrame;
					}
					doc.PreviewAnimation = !doc.PreviewAnimation;
					Application.InvalidateWindows();
				}
				doc.Container.IsRunning = doc.PreviewAnimation;
				if (doc.PreviewAnimation) {
					Application.InvalidateWindows();
				}
			}
		}
	}
}