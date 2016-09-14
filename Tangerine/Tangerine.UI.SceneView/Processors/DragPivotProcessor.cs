using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragPivotProcessor : IProcessor
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				var widgets = Utils.UnlockedWidgets();
				Quadrangle hull;
				Vector2 pivot;
				if (
					sv.Input.IsKeyPressed(Key.LControl) &&
					Utils.CalcHullAndPivot(widgets, sv.Scene, out hull, out pivot) &&
					(pivot - sv.MousePosition).Length < 10)
				{
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
						yield return Drag();
					}
				}
				yield return null;
			}
		}

		enum DragDirection
		{
			Any, Horizontal, Vertical
		}

		IEnumerator<object> Drag()
		{
			sv.Input.CaptureMouse();
			var initialMousePos = sv.MousePosition;
			var widgets = Utils.UnlockedWidgets().ToList();
			var dragDirection = DragDirection.Any;
			var positions = widgets.Select(i => i.Position).ToList();
			var pivots = widgets.Select(i => i.Pivot).ToList();
			var hull = CalcWidgetsHull(widgets);
			while (sv.Input.IsMousePressed()) {
				Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				var curMousePos = sv.MousePosition;
				var shiftPressed = sv.Input.IsKeyPressed(Key.LShift);
				if (shiftPressed && dragDirection != DragDirection.Any) {
					if (dragDirection == DragDirection.Horizontal) {
						curMousePos.Y = initialMousePos.Y;
					} else if (dragDirection == DragDirection.Vertical) {
						curMousePos.X = initialMousePos.X;
					}
				}
				SnapMousePosToHull(hull, ref curMousePos);
				if (shiftPressed && dragDirection == DragDirection.Any && (curMousePos - initialMousePos).Length > 5) {
					var d = curMousePos - initialMousePos;
					dragDirection = d.X.Abs() > d.Y.Abs() ? DragDirection.Horizontal : DragDirection.Vertical;
				}
				for (int i = 0; i < widgets.Count; i++) {
					var widget = widgets[i];
					var transform = sv.Scene.CalcTransitionToSpaceOf(widget);
					var dragDelta = curMousePos * transform - initialMousePos * transform;
					var deltaPivot = dragDelta / widget.Size;
					var deltaPos = Vector2.RotateDeg(dragDelta * widget.Scale, widget.Rotation);
					Core.Operations.SetAnimableProperty.Perform(widget, "Pivot", pivots[i] + deltaPivot.Snap(Vector2.Zero));
					Core.Operations.SetAnimableProperty.Perform(widget, "Position", positions[i] + deltaPos.Snap(Vector2.Zero));
				}
				yield return null;
			}
			sv.Input.ReleaseMouse();
		}

		static void SnapMousePosToHull(Quadrangle hull, ref Vector2 mousePos)
		{
			for (int i = 0; i < 4; i++) {
				if (HitTestSpecialPoint(mousePos, hull[i])) {
					mousePos = hull[i];
				}
				var p = (hull[(i + 1) % 4] + hull[i]) / 2;
				if (HitTestSpecialPoint(mousePos, p)) {
					mousePos = p;
				}
			}
			var center = (hull[0] + hull[2]) / 2;
			if (HitTestSpecialPoint(mousePos, center)) {
				mousePos = center;
			}
		}

		static bool HitTestSpecialPoint(Vector2 mousePos, Vector2 point)
		{
			return (mousePos - point).Length < 10;
		}

		Quadrangle CalcWidgetsHull(List<Widget> widgets)
		{
			Quadrangle hull;
			Vector2 pivot;
			Utils.CalcHullAndPivot(widgets, sv.Scene, out hull, out pivot);
			return hull;
		}
	}
}