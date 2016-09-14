using System;
using System.Linq;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	class MouseScrollProcessor : Core.IProcessor
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				if (sv.Input.WasMousePressed() && CommonWindow.Current.Input.IsKeyPressed(Key.Space)) {
					var initialMouse = sv.Input.MousePosition;
					var initialPosition = sv.Scene.Position;
					sv.Input.CaptureMouse();
					sv.Input.ConsumeKey(Key.Mouse0);
					while (sv.Input.IsMousePressed()) {
						sv.Scene.Position = (sv.Input.MousePosition - initialMouse) + initialPosition;
						yield return null;
					}
					sv.Input.ReleaseMouse();
				}
				if (sv.Input.WasKeyPressed(Key.MouseWheelDown)) {
					ZoomCanvas(-1);
				} else if (sv.Input.WasKeyPressed(Key.MouseWheelUp)) {
					ZoomCanvas(1);
				}
				yield return null;
			}
		}

		readonly List<float> zoomTable = new List<float> {
			0.001f, 0.0025f, 0.005f, 0.01f, 0.025f, 0.05f, 0.10f, 
			0.15f, 0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f, 3f,
			4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f,
			12f, 13f, 14f, 15f, 16f
		};

		void ZoomCanvas(int advance)
		{
			var i = zoomTable.IndexOf(sv.Scene.Scale.X);
			if (i < 0) {
				throw new InvalidOperationException();
			}
			var prevZoom = zoomTable[i];
			var zoom = zoomTable[(i + advance).Clamp(0, zoomTable.Count - 1)];
			var p = sv.MousePosition;
			sv.Scene.Scale = zoom * Vector2.One;
			sv.Scene.Position -= p * (zoom - prevZoom);
		}
	}
}