using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ResizeProcessor : IProcessor
	{
		SceneView sceneView => SceneView.Instance;
		Widget canvas => SceneView.Instance.CanvasWidget;
		WidgetInput input => SceneView.Instance.InputArea.Input;

		public IEnumerator<object> Loop()
		{
			while (true) {
				if (input.WasMousePressed()) {
					var widgets = Utils.UnlockedWidgets();
					if (widgets.Count > 0) {
						Quadrangle hull;
						Vector2 pivot;
						Utils.CalcHullAndPivot(widgets, canvas, out hull, out pivot);
						for (int i = 0; i < 4; i++) {
							if (HitTestStretcher(hull[i])) {
								yield return Resize(i * 2);
							}
							if (HitTestStretcher((hull[(i + 1) % 4] + hull[i]) / 2)) {
								yield return Resize(i * 2 + 1);
							}
						}
					}
				}
				yield return null;
			}
		}

		bool HitTestStretcher(Vector2 stretcher)
		{
			return (stretcher - canvas.Input.LocalMousePosition).Length < 10;
		}

		IEnumerator<object> Resize(int stretcher)
		{
			input.CaptureMouse();
			input.ConsumeKey(Key.Mouse0);
			var widgets = Utils.UnlockedWidgets();
			var mousePos = canvas.Input.LocalMousePosition;
			while (input.IsMousePressed()) {
				var mouseDelta = canvas.Input.LocalMousePosition - mousePos;
				var proportional = input.IsKeyPressed(Key.LShift);
				foreach (var widget in widgets) {
					ProcessWidget(widget, stretcher, mouseDelta, proportional);
				}
				mousePos = canvas.Input.LocalMousePosition;
				yield return null;
			}
			input.ReleaseMouse();
		}

		readonly Vector2[] sizeTable = {
			new Vector2(-1, -1),
			new Vector2(0, -1),
			new Vector2(1, -1),
			new Vector2(1, 0),
			new Vector2(1, 1),
			new Vector2(0, 1),
			new Vector2(-1, 1),
			new Vector2(-1, 0),
		};

		readonly Vector2[] positionTable = {
			new Vector2(1, 1),
			new Vector2(0, 1),
			new Vector2(0, 1),
			new Vector2(0, 0),
			new Vector2(0, 0),
			new Vector2(0, 0),
			new Vector2(1, 0),
			new Vector2(1, 0),
		};

		void ProcessWidget(Widget widget, int stretcher, Vector2 mouseDelta, bool proportional)
		{
			var rotatedMouseDelta = Vector2.RotateDeg(mouseDelta, -widget.Rotation);
			var deltaSize = rotatedMouseDelta * sizeTable[stretcher];
			var deltaPosition = rotatedMouseDelta * positionTable[stretcher];
			if (proportional) {
				if(stretcher == 1 || stretcher == 5) {
					deltaSize.Y = deltaSize.X;
					deltaPosition.Y = deltaPosition.X;
				} else {
					deltaSize.X = deltaSize.Y;
					deltaPosition.X = deltaPosition.Y;
				}
			}
			var newSize = widget.Size + deltaSize / widget.Scale;
			var newPos = widget.Position + Vector2.RotateDeg(deltaPosition + widget.Pivot * deltaSize, widget.Rotation);
			Core.Operations.SetAnimableProperty.Perform(widget, "Position", newPos);
			Core.Operations.SetAnimableProperty.Perform(widget, "Size", newSize);
		}
	}
}