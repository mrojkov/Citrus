using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragSplinePoint3DProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			while (true) {
				yield return null;
				var spline = Document.Current.Container as Spline3D;
				if (spline == null)
					continue;
				var viewport = spline.GetViewport();
				var points = Document.Current.SelectedNodes().Editable().OfType<SplinePoint3D>();
				var viewportToScene = viewport.CalcTransitionToSpaceOf(SceneView.Instance.Scene);
				foreach (var point in points) {
					var p = (Vector2)viewport.WorldToViewportPoint(point.Position * spline.GlobalTransform) * viewportToScene;
					if (SceneView.Instance.HitTestControlPoint(p)) {
						Utils.ChangeCursorIfDefault(MouseCursor.Hand);
						if (SceneView.Instance.Input.ConsumeKeyPress(Key.Mouse0)) {
							yield return Drag(point);
						}
					}
				}
			}
		}

		IEnumerator<object> Drag(SplinePoint3D point)
		{
			var input = SceneView.Instance.Input;
			input.CaptureMouse();
			Document.Current.History.BeginTransaction();
			Vector3? posCorrection = null;
			try {
				var spline = (Spline3D)Document.Current.Container;
				var viewport = spline.GetViewport();
				var initialMouse = input.MousePosition;
				var dragDirection = DragDirection.Any;
				var xyPlane = new Plane(new Vector3(0, 0, 1), 0).Transform(spline.GlobalTransform);
				// Drag point in the plane parallel to XY plane in the spline coordinate system.
				xyPlane.D = -xyPlane.DotCoordinate(point.CalcGlobalPosition());
				while (input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var currentMouse = input.MousePosition;
					var shiftPressed = input.IsKeyPressed(Key.Shift);
					if (shiftPressed && dragDirection != DragDirection.Any) {
						if (dragDirection == DragDirection.Horizontal) {
							currentMouse.Y = initialMouse.Y;
						} else if (dragDirection == DragDirection.Vertical) {
							currentMouse.X = initialMouse.X;
						}
					}
					var ray = viewport.ScreenPointToRay(currentMouse);
					if (shiftPressed && dragDirection == DragDirection.Any) {
						if ((currentMouse - initialMouse).Length > 5 / SceneView.Instance.Scene.Scale.X) {
							var mouseDelta = currentMouse - initialMouse;
							dragDirection = mouseDelta.X.Abs() > mouseDelta.Y.Abs() ?
								DragDirection.Horizontal : DragDirection.Vertical;
						}
					}
					var distance = ray.Intersects(xyPlane);
					if (distance.HasValue) {
						var pos = (ray.Position + ray.Direction * distance.Value) * spline.GlobalTransform.CalcInverted();
						posCorrection = posCorrection ?? point.Position - pos;
						Core.Operations.SetAnimableProperty.Perform(point, nameof(SplinePoint3D.Position), pos + posCorrection.Value);
					}
					yield return null;
				}
			} finally {
				input.ReleaseMouse();
				Document.Current.History.EndTransaction();
			}
		}

		enum DragDirection
		{
			Any, Horizontal, Vertical
		}
	}
}