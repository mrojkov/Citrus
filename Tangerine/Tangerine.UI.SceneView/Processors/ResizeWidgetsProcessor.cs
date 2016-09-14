using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ResizeWidgetsProcessor : IProcessor
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				var widgets = Utils.UnlockedWidgets();
				Quadrangle hull;
				Vector2 pivot;
				if (Utils.CalcHullAndPivot(widgets, sv.Scene, out hull, out pivot)) {
					for (int i = 0; i < 4; i++) {
						var a = hull[i];
						if (HitTestControlPoint(a)) {
							Utils.ChangeCursorIfDefault(MouseCursor.SizeNS);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Resize(i * 2);
							}
						}
						var b = hull[(i + 1) % 4];
						if (HitTestControlPoint((a + b) / 2)) {
							var cursor = (b.X - a.X).Abs() > (b.Y - a.Y).Abs() ? MouseCursor.SizeNS : MouseCursor.SizeWE;
							Utils.ChangeCursorIfDefault(cursor);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Resize(i * 2 + 1);
							}
						}
					}
				}
				yield return null;
			}
		}

		bool HitTestControlPoint(Vector2 controlPoint)
		{
			return (controlPoint - sv.MousePosition).Length < 6;
		}

		IEnumerator<object> Resize(int controlPointIndex)
		{
			var cursor = WidgetContext.Current.MouseCursor;
			sv.Input.CaptureMouse();
			var widgets = Utils.UnlockedWidgets();
			var mousePos = sv.MousePosition;
			while (sv.Input.IsMousePressed()) {
				Utils.ChangeCursorIfDefault(cursor);
				var mouseDelta = sv.MousePosition - mousePos;
				var proportional = sv.Input.IsKeyPressed(Key.LShift);
				foreach (var widget in widgets) {
					ProcessWidget(widget, controlPointIndex, mouseDelta, proportional);
				}
				mousePos = sv.MousePosition;
				yield return null;
			}
			sv.Input.ReleaseMouse();
		}

		readonly Vector2[] directionLookup = {
			new Vector2(-1, -1),
			new Vector2(0, -1),
			new Vector2(1, -1),
			new Vector2(1, 0),
			new Vector2(1, 1),
			new Vector2(0, 1),
			new Vector2(-1, 1),
			new Vector2(-1, 0),
		};

		readonly Vector2[] positionLookup = {
			new Vector2(1, 1),
			new Vector2(0, 1),
			new Vector2(0, 1),
			new Vector2(0, 0),
			new Vector2(0, 0),
			new Vector2(0, 0),
			new Vector2(1, 0),
			new Vector2(1, 0),
		};

		void ProcessWidget(Widget widget, int controlPointIndex, Vector2 mouseDelta, bool proportional)
		{
			var rotatedMouseDelta = Vector2.RotateDeg(mouseDelta, -widget.Rotation);
			var deltaSize = rotatedMouseDelta * directionLookup[controlPointIndex];
			var deltaPosition = rotatedMouseDelta * positionLookup[controlPointIndex];
			if (proportional) {
				if(controlPointIndex == 1 || controlPointIndex == 5) {
					deltaSize.Y = deltaSize.X;
					deltaPosition.Y = deltaPosition.X;
				} else {
					deltaSize.X = deltaSize.Y;
					deltaPosition.X = deltaPosition.Y;
				}
			}
			var size = widget.Size + (deltaSize / widget.Scale).Snap(Vector2.Zero);
			var position = widget.Position + Vector2.RotateDeg(deltaPosition + widget.Pivot * deltaSize, widget.Rotation).Snap(Vector2.Zero);
			Core.Operations.SetAnimableProperty.Perform(widget, "Position", position);
			Core.Operations.SetAnimableProperty.Perform(widget, "Size", size);
		}
	}
}