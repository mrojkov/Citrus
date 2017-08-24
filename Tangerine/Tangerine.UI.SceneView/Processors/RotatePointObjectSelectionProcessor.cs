using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class RotatePointObjectSelectionProcessor : ITaskProvider
	{
		private SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				var selectedPointObjects = Document.Current.SelectedNodes().Editable().OfType<PointObject>().ToList();
				if (selectedPointObjects.Count() > 1) {
					var t = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(sv.Scene);
					var hull = Utils.CalcAABB(Document.Current.SelectedNodes().Editable().OfType<PointObject>(), true);
					var size = Document.Current.Container.AsWidget.Size;
					var cornerOffset = PointObjectSelectionComponent.cornerOffset;
					var expandedHull = new Quadrangle();
					for (int i = 0; i < 4; i++) {
						expandedHull[i] = hull[i] * size * t;
					}

					for (int i = 0; i < 4; i++) {
						var next = (i + 1) % 4;
						var prev = (i + 3) % 4;
						var dir1 = expandedHull[i] - expandedHull[next];
						var dir2 = expandedHull[i] - expandedHull[prev];
						var corner = (dir1.Normalized + dir2.Normalized);
						if (sv.HitTestControlPoint(expandedHull[i] + corner * cornerOffset / sv.Scene.Scale)) {
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

		IEnumerator<object> Rotate(Quadrangle bounds, List<PointObject> points)
		{
			sv.Input.CaptureMouse();
			Document.Current.History.BeginTransaction();

			try {
				var t = sv.Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
				var center = (bounds.V1 + bounds.V3) / 2;
				var size = Document.Current.Container.AsWidget.Size;
				var scale = Document.Current.Container.AsWidget.Scale;
				var mousePosPrev = (sv.MousePosition * t - center * size) / size;
				var rotation = 0f;
				var prevAngle = rotation;
				while (sv.Input.IsMousePressed()) {
					if (!sv.Components.Contains<PointObjectSelectionComponent>()) {
						sv.Components.Add(new PointObjectSelectionComponent {
							СurrentBounds = bounds,
						});
					}
					Utils.ChangeCursorIfDefault(Cursors.Rotate);
					var a = mousePosPrev;
					var b = (sv.MousePosition * t - center * size) / size;
					mousePosPrev = b;
					var angle = 0f;
					if (a.Length > Mathf.ZeroTolerance && b.Length > Mathf.ZeroTolerance) {
						angle = Mathf.Wrap360(b.Atan2Deg - a.Atan2Deg);
						rotation += angle;
					}
					if (Math.Abs(angle) > Mathf.ZeroTolerance) {
						var roundedAngle = Utils.RoundTo(rotation, 15);
						var effectiveAngle = sv.Input.IsKeyPressed(Key.Shift) ? roundedAngle - prevAngle : angle;
						prevAngle = roundedAngle;
						for (int i = 0; i < 4; i++) {
							bounds[i] = Vector2.RotateDeg(bounds[i] - center, effectiveAngle) + center;
						}
						sv.Components.Get<PointObjectSelectionComponent>().СurrentBounds = bounds;
						for (var i = 0; i < points.Count; i++) {
							var offset = center - points[i].Offset / size;
							var position = Vector2.RotateDeg((points[i].Position - offset), effectiveAngle) + offset;
							Core.Operations.SetAnimableProperty.Perform(points[i], nameof(PointObject.Position), position);
							if (points[i] is SplinePoint) {
								Core.Operations.SetAnimableProperty.Perform(
									points[i],
									nameof(SplinePoint.TangentAngle),
									(points[i] as SplinePoint).TangentAngle + effectiveAngle);
							}
						}
					}

					yield return null;
				}
			} finally {
				sv.Input.ReleaseMouse();
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
				sv.Components.Remove<PointObjectSelectionComponent>();
				Window.Current.Invalidate();
			}
		}
	}
}
