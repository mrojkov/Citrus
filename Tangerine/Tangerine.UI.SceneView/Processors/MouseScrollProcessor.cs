using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	class MouseScrollProcessor : Core.IProcessor
	{
		public IEnumerator<object> Loop()
		{
			var sv = SceneView.Instance;
			while (true) {
				if (sv.Input.WasMousePressed() && CommonWindow.Current.Input.IsKeyPressed(Key.Space)) {
					var initialMouse = sv.Input.MousePosition;
					var initialPosition = sv.Canvas.Position;
					sv.Input.CaptureMouse();
					while (sv.Input.IsMousePressed()) {
						sv.Canvas.Position = (sv.Input.MousePosition - initialMouse) + initialPosition;
						yield return null;
					}
					sv.Input.ReleaseMouse();
				}
				yield return null;
			}
		}
	}
}