using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class RotatePointObjectSelectionProcessor : ITaskProvider
	{
		private SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				if (!SceneView.Instance.InputArea.IsMouseOverThisOrDescendant()) {
					yield return null;
					continue;
				}
				var selectedPointObjects = Document.Current.SelectedNodes().Editable().OfType<PointObject>().ToList();
				if (selectedPointObjects.Count() > 1) {
					Utils.CalcHullAndPivot(selectedPointObjects, out var hull, out _);
					hull = hull.Transform(Document.Current.Container.AsWidget.LocalToWorldTransform);
					var expandedBoundsInSceneCoords = PointObjectsPresenter.ExpandAndTranslateToSpaceOf(hull, Document.Current.Container.AsWidget, sv) *
						sv.Frame.CalcTransitionToSpaceOf(sv.Scene);
					for (var i = 0; i < 4; i++) {
						if (sv.HitTestControlPoint(expandedBoundsInSceneCoords[i])) {
							Utils.ChangeCursorIfDefault(Cursors.Rotate);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Rotate(hull, selectedPointObjects);
							}
						}
					}
				}
				yield return null;
			}
		}

		IEnumerator<object> Rotate(Quadrangle hull, List<PointObject> points)
		{
			using (Document.Current.History.BeginTransaction()) {
				var t = Document.Current.Container.AsWidget.LocalToWorldTransform.CalcInversed();
				hull *= Matrix32.Scaling(Vector2.One/ Document.Current.Container.AsWidget.Size);
				var center = (hull.V1 + hull.V3) / 2;
				var size = Document.Current.Container.AsWidget.Size;
				var mousePosInitial = (sv.MousePosition * t - center * size) / size;
				var rotation = 0f;
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();
					Utils.ChangeCursorIfDefault(Cursors.Rotate);
					var b = (sv.MousePosition * t - center * size) / size;
					var angle = 0f;
					if (mousePosInitial.Length > Mathf.ZeroTolerance && b.Length > Mathf.ZeroTolerance) {
						angle = Mathf.Wrap180(b.Atan2Deg - mousePosInitial.Atan2Deg);
						rotation = angle;
					}
					if (Math.Abs(angle) > Mathf.ZeroTolerance) {
						var effectiveAngle = sv.Input.IsKeyPressed(Key.Shift) ? Utils.RoundTo(rotation, 15) : angle;
						Quadrangle newBounds = new Quadrangle();
						for (int i = 0; i < 4; i++) {
							newBounds[i] = Vector2.RotateDeg(hull[i] - center, effectiveAngle) + center;
						}
						for (var i = 0; i < points.Count; i++) {
							var offset = center - points[i].Offset / size;
							var position = Vector2.RotateDeg((points[i].Position - offset), effectiveAngle) + offset;
							Core.Operations.SetAnimableProperty.Perform(points[i], nameof(PointObject.Position), position, CoreUserPreferences.Instance.AutoKeyframes);
							if (points[i] is SplinePoint) {
								Core.Operations.SetAnimableProperty.Perform(
									points[i],
									nameof(SplinePoint.TangentAngle),
									(points[i] as SplinePoint).TangentAngle - effectiveAngle,
									CoreUserPreferences.Instance.AutoKeyframes
								);
							}
						}
					}

					yield return null;
				}
				sv.Input.ConsumeKey(Key.Mouse0);
				Window.Current.Invalidate();
				Document.Current.History.CommitTransaction();
			}
		}
	}
}
