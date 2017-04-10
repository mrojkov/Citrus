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
				var points = Document.Current.SelectedNodes().Editable().OfType<SplinePoint3D>();
				foreach (var point in points) {
					if (HitTestControlPoint(spline, point.Position)) {
						Utils.ChangeCursorIfDefault(MouseCursor.Hand);
						if (SceneView.Instance.Input.ConsumeKeyPress(Key.Mouse0)) {
							yield return DragPoint(point);
						}
					}
					for (int i = 0; i < 2; i++) {
						if (HitTestControlPoint(spline, point.Position + GetTangent(point, i))) {
							Utils.ChangeCursorIfDefault(MouseCursor.Hand);
							if (SceneView.Instance.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return DragTangent(point, i);
							}
						}
					}
				}
			}
		}

		Vector3 GetTangent(SplinePoint3D point, int index) => index == 0 ? point.TangentA : point.TangentB;

		bool HitTestControlPoint(Spline3D spline, Vector3 pointInSplineCoordinates)
		{
			var viewport = spline.GetViewport();
			var viewportToScene = viewport.CalcTransitionToSpaceOf(SceneView.Instance.Scene);
			var screenPoint = (Vector2)viewport.WorldToViewportPoint(pointInSplineCoordinates * spline.GlobalTransform) * viewportToScene;
			return SceneView.Instance.HitTestControlPoint(screenPoint);
		}

		IEnumerator<object> DragPoint(SplinePoint3D point)
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
				var plane = CalcPlane(spline, point.CalcGlobalPosition());
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
					var distance = ray.Intersects(plane);
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

		IEnumerator<object> DragTangent(SplinePoint3D point, int tangentIndex)
		{
			var input = SceneView.Instance.Input;
			input.CaptureMouse();
			Document.Current.History.BeginTransaction();
			Vector3? posCorrection = null;
			try {
				var spline = (Spline3D)Document.Current.Container;
				var viewport = spline.GetViewport();
				var plane = CalcPlane(spline, point.CalcGlobalPosition() + GetTangent(point, tangentIndex));
				var tangentsAreEqual = (point.TangentA + point.TangentB).Length < 0.1f;
				while (input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var ray = viewport.ScreenPointToRay(input.MousePosition);
					var distance = ray.Intersects(plane);
					if (distance.HasValue) {
						var pos = (ray.Position + ray.Direction * distance.Value) * spline.GlobalTransform.CalcInverted() - point.Position;
						posCorrection = posCorrection ?? GetTangent(point, tangentIndex) - pos;
						Core.Operations.SetAnimableProperty.Perform(point, GetTangentPropertyName(tangentIndex), pos + posCorrection.Value);
						if (input.IsKeyPressed(Key.Shift) ^ tangentsAreEqual) {
							Core.Operations.SetAnimableProperty.Perform(point, GetTangentPropertyName(1 - tangentIndex), -(pos + posCorrection.Value));
						}
					}
					yield return null;
				}
			} finally {
				input.ReleaseMouse();
				Document.Current.History.EndTransaction();
			}
		}

		static string GetTangentPropertyName(int index) => index == 0 ? nameof(SplinePoint3D.TangentA) : nameof(SplinePoint3D.TangentB); 
  
		static Plane CalcPlane(Spline3D spline, Vector3 point)
		{
			var xyPlane = new Plane(new Vector3(0, 0, 1), 0).Transform(spline.GlobalTransform);
			// Drag point in the plane parallel to XY plane in the spline coordinate system.
			xyPlane.D = -xyPlane.DotCoordinate(point);
			return xyPlane;
		}

		enum DragDirection
		{
			Any, Horizontal, Vertical
		}
	}
}