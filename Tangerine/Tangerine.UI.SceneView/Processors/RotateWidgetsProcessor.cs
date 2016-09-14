using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class RotateWidgetsProcessor : IProcessor
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
						if (HitTestControlPoint(hull[i])) {
							Utils.ChangeCursorIfDefault(MouseCursor.Hand);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Rotate(pivot);
							}
						}
					}
				}
				yield return null;
			}
		}

		bool HitTestControlPoint(Vector2 controlPoint)
		{
			return (controlPoint - sv.MousePosition).Length < 10;
		}

		IEnumerator<object> Rotate(Vector2 pivot)
		{
			sv.Input.CaptureMouse();
			var widgets = Utils.UnlockedWidgets().ToList();
			var mousePos = sv.MousePosition;
			while (sv.Input.IsMousePressed()) {
				Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				var a = mousePos - pivot;
				var b = sv.MousePosition - pivot;
				float angle = 0;
				if (a.Length > Mathf.ZeroTolerance && b.Length > Mathf.ZeroTolerance) {
					angle = b.Atan2Deg - a.Atan2Deg;
					if (angle > 180) {
						angle -= 360;
					}
					if (angle < -180) {
						angle += 360;
					}
				}
				foreach (var widget in widgets) {
					RotateWidget(widget, pivot, angle);
				}
				mousePos = sv.MousePosition;
				yield return null;
			}
			sv.Input.ReleaseMouse();
		}

		void RotateWidget(Widget widget, Vector2 pivot, float angle)
		{
			var transform = widget.CalcTransitionToSpaceOf(sv.Scene).CalcInversed();
			var newPivot = ((transform * pivot) / widget.Size).Snap(widget.Pivot);
			var deltaPos = Vector2.RotateDeg((newPivot - widget.Pivot) * (widget.Scale * widget.Size), widget.Rotation);
			var newPos = widget.Position + deltaPos.Snap(Vector2.Zero);
			var newRotation = widget.Rotation + angle.Snap(0);
			Core.Operations.SetAnimableProperty.Perform(widget, "Position", newPos);
			Core.Operations.SetAnimableProperty.Perform(widget, "Pivot", newPivot);
			Core.Operations.SetAnimableProperty.Perform(widget, "Rotation", newRotation);
		}
	}
}