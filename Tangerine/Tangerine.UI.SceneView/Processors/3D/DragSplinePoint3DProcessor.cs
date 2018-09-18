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
				if (!SceneView.Instance.InputArea.IsMouseOverThisOrDescendant()) {
					yield return null;
					continue;
				}
				yield return null;
				var spline = Document.Current.Container as Spline3D;
				if (spline == null)
					continue;
				var points = Document.Current.SelectedNodes().Editable().OfType<SplinePoint3D>();
				foreach (var point in points) {
					if (HitTestControlPoint(spline, point.Position)) {
						Utils.ChangeCursorIfDefault(MouseCursor.Hand);
						if (SceneView.Instance.Input.ConsumeKeyPress(Key.Mouse0)) {
							yield return DragPoints(points);
							break;
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
			var viewport = spline.Viewport;
			var viewportToScene = viewport.CalcTransitionToSpaceOf(SceneView.Instance.Scene);
			var screenPoint = (Vector2)viewport.WorldToViewportPoint(pointInSplineCoordinates * spline.GlobalTransform) * viewportToScene;
			return SceneView.Instance.HitTestControlPoint(screenPoint);
		}

		IEnumerator<object> DragPoints(IEnumerable<SplinePoint3D> points)
		{
			var input = SceneView.Instance.Input;
			var offsets = new Vector3?[points.Count()];
			using (Document.Current.History.BeginTransaction()) {
				var spline = (Spline3D)Document.Current.Container;
				var viewport = spline.Viewport;
				var initialMouse = input.MousePosition;
				var dragDirection = DragDirection.Any;
				while (input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();

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
					int i = 0;
					foreach (var p in points) {
						var plane = CalcPlane(spline, p.Position);
						var distance = ray.Intersects(plane);
						if (distance.HasValue) {
							var pos = (ray.Position + ray.Direction * distance.Value) * spline.GlobalTransform.CalcInverted();
							offsets[i] = offsets[i] ?? p.Position - pos;
							Core.Operations.SetAnimableProperty.Perform(p, nameof(SplinePoint3D.Position), pos + offsets[i].Value, CoreUserPreferences.Instance.AutoKeyframes);
							i++;
						}
					}
					yield return null;
				}
				Document.Current.History.CommitTransaction();
			}
		}

		IEnumerator<object> DragTangent(SplinePoint3D point, int tangentIndex)
		{
			var input = SceneView.Instance.Input;
			Vector3? posCorrection = null;
			using (Document.Current.History.BeginTransaction()) {
				var spline = (Spline3D)Document.Current.Container;
				var viewport = spline.Viewport;
				var plane = CalcPlane(spline, point.Position + GetTangent(point, tangentIndex));
				var tangentsAreEqual = (point.TangentA + point.TangentB).Length < 0.1f;
				while (input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();

					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var ray = viewport.ScreenPointToRay(input.MousePosition);
					var distance = ray.Intersects(plane);
					if (distance.HasValue) {
						var pos = (ray.Position + ray.Direction * distance.Value) * spline.GlobalTransform.CalcInverted() - point.Position;
						posCorrection = posCorrection ?? GetTangent(point, tangentIndex) - pos;
						Core.Operations.SetAnimableProperty.Perform(point, GetTangentPropertyName(tangentIndex), pos + posCorrection.Value, CoreUserPreferences.Instance.AutoKeyframes);
						if (input.IsKeyPressed(Key.Shift) ^ tangentsAreEqual) {
							Core.Operations.SetAnimableProperty.Perform(point, GetTangentPropertyName(1 - tangentIndex), -(pos + posCorrection.Value), CoreUserPreferences.Instance.AutoKeyframes);
						}
					}
					yield return null;
				}
				Document.Current.History.CommitTransaction();
			}
		}

		static string GetTangentPropertyName(int index) => index == 0 ? nameof(SplinePoint3D.TangentA) : nameof(SplinePoint3D.TangentB);

		static Plane CalcPlane(Spline3D spline, Vector3 point)
		{
			var m = spline.GlobalTransform;
			var normal = m.TransformNormal(new Vector3(0, 0, 1)).Normalized;
			var d = -Vector3.DotProduct(m.TransformVector(point), normal);
			return new Plane(normal, d);
		}

		enum DragDirection
		{
			Any, Horizontal, Vertical
		}
	}
}
