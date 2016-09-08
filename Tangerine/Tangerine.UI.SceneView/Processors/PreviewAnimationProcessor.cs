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
				if (input.ConsumeKeyPress(KeyBindings.SceneViewKeys.PreviewAnimation)) {
					if (doc.Container.IsRunning) {
						doc.Container.AnimationFrame = savedFrame;
					} else {
						savedFrame = doc.Container.AnimationFrame;
					}
					doc.Container.IsRunning = !doc.Container.IsRunning;
					InvalidateWindows();
				}
				if (doc.Container.IsRunning) {
					InvalidateWindows();
				}
				yield return null;
			}
		}

		void InvalidateWindows()
		{
			foreach (var window in Application.Windows) {
				window.Invalidate();
			}
		}
	}
}