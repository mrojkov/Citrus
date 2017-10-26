using System;
using System.Linq;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	class MouseScrollProcessor : Core.ITaskProvider
	{
		SceneView sv => SceneView.Instance;
		static Vector2 mouseStartPos;

		public IEnumerator<object> Task()
		{
			var prevPos = sv.Input.MousePosition;
			while (true) {
				if (sv.Input.WasMousePressed(0) && CommonWindow.Current.Input.IsKeyPressed(Key.Space) || sv.Input.WasMousePressed(2)) {
					var initialMouse = sv.Input.MousePosition;
					var initialPosition = sv.Scene.Position;
					sv.Input.ConsumeKey(Key.Mouse0);
					sv.Input.ConsumeKey(Key.Mouse2);
					while (sv.Input.IsMousePressed(0) || sv.Input.IsMousePressed(2)) {
						sv.Scene.Position = (sv.Input.MousePosition - initialMouse) + initialPosition;
						yield return null;
					}
				}

				if (sv.Input.WasKeyPressed(Key.Alt) || sv.Input.WasMousePressed(1)) {
					mouseStartPos = sv.MousePosition;
				}

				if (sv.Input.WasKeyPressed(Key.MouseWheelDown)) {
					ZoomCanvas(-1);
				} else if (sv.Input.WasKeyPressed(Key.MouseWheelUp)) {
					ZoomCanvas(1);
				} else if (sv.Input.IsKeyPressed(Key.Alt) && sv.Input.IsKeyPressed(Key.Mouse1)) {
					var delta = (sv.Input.MousePosition - prevPos).X;
					var size = (float)Math.Sqrt(sv.Scene.Size.SqrLength);
					ZoomCanvas(delta / size);
				}
				prevPos = sv.Input.MousePosition;
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
			var i = FindNearest(sv.Scene.Scale.X, 0, zoomTable.Count - 1);
			if (i < 0) {
				throw new InvalidOperationException();
			}
			var prevZoom = sv.Scene.Scale.X;
			var zoom = zoomTable[(i + advance).Clamp(0, zoomTable.Count - 1)];
			var p = sv.MousePosition;
			sv.Scene.Scale = zoom * Vector2.One;
			sv.Scene.Position -= p * (zoom - prevZoom);
		}

		void ZoomCanvas(float delta)
		{
			var zoom = sv.Scene.Scale.X + delta;
			if (zoom < zoomTable.First() || zoom > zoomTable.Last()) {
				return;
			}
			sv.Scene.Scale = zoom * Vector2.One;
			sv.Scene.Position -= mouseStartPos * delta;
		}

		private int FindNearest(float x, int left, int right)
		{
			if (right - left == 1) {
				return left;
			}
			var idx = left + (right - left) / 2;
			return x < zoomTable[idx] ? FindNearest(x, left, idx) : FindNearest(x, idx, right);
		}
	}
}