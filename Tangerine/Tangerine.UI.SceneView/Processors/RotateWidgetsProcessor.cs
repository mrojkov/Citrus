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
			var rotations = widgets.Select(i => i.Rotation).ToList();
			var mousePos = sv.MousePosition;
			float rotation = 0;
			foreach (var widget in widgets) {
				SetWidgetPivot(widget, pivot);
			}
			while (sv.Input.IsMousePressed()) {
				Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				var a = mousePos - pivot;
				var b = sv.MousePosition - pivot;
				if (a.Length > Mathf.ZeroTolerance && b.Length > Mathf.ZeroTolerance) {
					rotation += WrapAngle(b.Atan2Deg - a.Atan2Deg);
				}
				for (int i = 0; i < widgets.Count; i++) {
					SetWidgetRotation(widgets[i], rotations[i] + GetSnappedRotation(rotation));
				}
				mousePos = sv.MousePosition;
				yield return null;
			}
			sv.Input.ReleaseMouse();
		}

		static float WrapAngle(float angle)
		{
			if (angle > 180) {
				return angle - 360;
			}
			if (angle < -180) {
				return angle + 360;
			}
			return angle;
		}

		float GetSnappedRotation(float rotation)
		{
			if (sv.Input.IsKeyPressed(Key.LShift)) {
				return ((rotation / 15f).Round() * 15f).Snap(0);
			} else {
				return rotation.Snap(0);
			}
		}

		void SetWidgetPivot(Widget widget, Vector2 pivot)
		{
			var transform = sv.Scene.CalcTransitionToSpaceOf(widget);
			var newPivot = ((transform * pivot) / widget.Size).Snap(widget.Pivot);
			var deltaPos = Vector2.RotateDeg((newPivot - widget.Pivot) * (widget.Scale * widget.Size), widget.Rotation);
			var newPos = widget.Position + deltaPos.Snap(Vector2.Zero);
			Core.Operations.SetAnimableProperty.Perform(widget, "Position", newPos);
			Core.Operations.SetAnimableProperty.Perform(widget, "Pivot", newPivot);
		}

		void SetWidgetRotation(Widget widget, float rotation)
		{
			//var transform = sv.Scene.CalcTransitionToSpaceOf(widget);
			//var newPivot = ((transform * pivot) / widget.Size).Snap(widget.Pivot);
			//var deltaPos = Vector2.RotateDeg((newPivot - widget.Pivot) * (widget.Scale * widget.Size), widget.Rotation);
			//var newPos = widget.Position + deltaPos.Snap(Vector2.Zero);
			//Core.Operations.SetAnimableProperty.Perform(widget, "Position", newPos);
			//Core.Operations.SetAnimableProperty.Perform(widget, "Pivot", newPivot);
			Core.Operations.SetAnimableProperty.Perform(widget, "Rotation", rotation);
		}
	}
}