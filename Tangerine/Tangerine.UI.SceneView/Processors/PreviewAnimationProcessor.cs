using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	public class PreviewAnimationProcessor : Core.IProcessor
	{
		readonly WidgetInput input;

		public PreviewAnimationProcessor(WidgetInput input)
		{
			this.input = input;
		}

		public IEnumerator<object> Loop()
		{
			int savedFrame = 0;
			while (true) {
				var doc = Core.Document.Current;
				if (doc != null) {
					if (input.ConsumeKeyPress(KeyBindings.SceneViewKeys.PreviewAnimation)) {
						if (doc.Container.IsRunning) {
							doc.Container.AnimationFrame = savedFrame;
						} else {
							savedFrame = doc.Container.AnimationFrame;
						}
						doc.Container.IsRunning = !doc.Container.IsRunning;
						Application.InvalidateWindows();
					}
					if (doc.Container.IsRunning) {
						Application.InvalidateWindows();
					}
				}
				yield return null;
			}
		}
	}
}