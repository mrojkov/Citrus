using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	class MouseScrollProcessor : Core.IProcessor
	{
		public IEnumerator<object> Loop()
		{
			var sceneView = SceneView.Instance;
			var input = sceneView.InputArea.Input;
			while (true) {
				if (input.WasMousePressed() && Window.Current.Input.IsKeyPressed(Key.Space)) {
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