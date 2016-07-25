using System;
using System.Collections.Generic;

namespace Tangerine.UI.SceneView
{
	class MouseScrollProcessor : Core.IProcessor
	{
		SceneView sceneView => SceneView.Instance;

		public IEnumerator<object> Loop()
		{
			var input = sceneView.CanvasWidget.Input;
			while (true) {
				if (input.WasMousePressed() && sceneView.InputScreen.IsMouseOver()) {
					var initialMouse = input.MousePosition;
					var initialPosition = sceneView.CanvasWidget.Position;
					input.CaptureMouse();
					while (input.IsMousePressed()) {
						sceneView.CanvasWidget.Position = (input.MousePosition - initialMouse) + initialPosition;
						yield return null;
					}
					input.ReleaseMouse();
				}
				yield return null;
			}
		}
	}
}