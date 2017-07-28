using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class RescalePointObjectSelectionProcessor : ITaskProvider
	{
		private SceneView sv => SceneView.Instance;
		private List<Vector2> initialPointsPosition;
		private Quadrangle initialPointsBounds;
		private Vector2 initialMousePosition;

		public IEnumerator<object> Task()
		{
			while (true) {
				var points = Document.Current.SelectedNodes().Editable().OfType<PointObject>().ToList();
				if (points.Count > 1) {
					var hull = Utils.CalcAABB(Document.Current.SelectedNodes().Editable().OfType<PointObject>(), true);
					var t = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(sv.Scene);
					var size = Document.Current.Container.AsWidget.Size;
					var cornerOffset = PointObjectSelectionComponent.cornerOffset;
					var hullSize = hull[0] - hull[2];
					var expandedHull = new Quadrangle();
					for (int i = 0; i < 4; i++) {
						hull[i] = hull[i] * size * t;
					}
					for (int i = 0; i < 4; i++) {
						var next = (i + 1) % 4;
						var prev = (i + 3) % 4;
						var dir1 = hull[i] - hull[next];
						var dir2 = hull[i] - hull[prev];
						var corner = (dir1.Normalized + dir2.Normalized);
						expandedHull[i] = hull[i] + corner * cornerOffset / sv.Scene.Scale;
						if (hullSize.X != 0 && hullSize.Y != 0) {
							if (sv.HitTestResizeControlPoint(expandedHull[i])) {
								Utils.ChangeCursorIfDefault(MouseCursor.SizeNS);
								if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
									yield return Rescale(i * 2, MouseCursor.SizeNS, points);
								}
							}
						}
					}
					for (int i = 0; i < 4; i++) {
						if (hullSize.X == 0 && i % 2 == 1 || hullSize.Y == 0 && i % 2 == 0) {
							continue;
						}
						var a = expandedHull[i];
						var b = expandedHull[(i + 1) % 4];
						if (sv.HitTestResizeControlPoint((a + b) / 2)) {
							var cursor = MouseCursor.Default;
							if (hullSize.X == 0) {
								cursor = MouseCursor.SizeNS;
							} else if (hullSize.Y == 0) {
								cursor = MouseCursor.SizeWE;
							} else {
								cursor = (b.X - a.X).Abs() > (b.Y - a.Y).Abs() ? MouseCursor.SizeNS : MouseCursor.SizeWE;
							}
							Utils.ChangeCursorIfDefault(cursor);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Rescale(i * 2 + 1, cursor, points);
							}
						}
					}
				}
				yield return null;
			}
		}

		IEnumerator<object> Rescale(int controlPointIndex, MouseCursor cursor, List<PointObject> points)
		{
			sv.Input.CaptureMouse();
			var t = sv.Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
			Document.Current.History.BeginTransaction();
			try {
				Utils.ChangeCursorIfDefault(cursor);
				initialMousePosition = sv.MousePosition;
				initialPointsPosition = points.Select(w => w.Position).ToList();
				initialPointsBounds = Utils.CalcAABB(points);
				while (sv.Input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(cursor);
					RescaleHelper(
						points,
						controlPointIndex,
						sv.Input.IsKeyPressed(Key.Shift),
						sv.Input.IsKeyPressed(Key.Control));
					yield return null;
				}
			} finally {
				sv.Input.ReleaseMouse();
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
			}
		}

		void RescaleHelper(List<PointObject> points, int controlPointIndex, bool proportional, bool centerProportional)
		{
			var t = sv.Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
			t.T = Vector2.Zero;
			var transformedMouseDelta = (sv.MousePosition - initialMousePosition) * t / Document.Current.Container.AsWidget.Size;
			var origin = Vector2.Zero;
			var idx = (controlPointIndex + 4) % 8 / 2;
			var next = (idx + 1) % 4;
			var prev = (idx + 3) % 4;
			var axisX = initialPointsBounds[next] - initialPointsBounds[idx];
			var axisY = initialPointsBounds[prev] - initialPointsBounds[idx];
			if (controlPointIndex == 7 || controlPointIndex == 3 || controlPointIndex == 1 || controlPointIndex == 5) {
				origin = (initialPointsBounds[next] + initialPointsBounds[idx]) / 2;
			} else {
				origin = initialPointsBounds[idx];
			}
			if (axisX == Vector2.Zero) {
				axisX = new Vector2(-axisY.Y, axisY.X);
			}
			if (axisY == Vector2.Zero) {
				axisY = new Vector2(-axisX.Y, axisX.X);
			}
			var basis = new Matrix32(axisX, axisY, centerProportional ? (initialPointsBounds.V1 + initialPointsBounds.V3) / 2 : origin);
			var basisInversed = basis.CalcInversed();
			var deltaSize = basisInversed * transformedMouseDelta - Vector2.Zero * basisInversed;
			deltaSize = (deltaSize * (controlPointIndex % 2 == 0 ? Vector2.One : Vector2.Down)).Snap(Vector2.Zero);
			if (deltaSize == Vector2.Zero) {
				return;
			}
			var avSize = 0.5f * (deltaSize.X + deltaSize.Y);
			if (proportional) {
				if (controlPointIndex == 7 || controlPointIndex == 3 || controlPointIndex == 1 || controlPointIndex == 5) {
					deltaSize.X = deltaSize.Y;
				} else {
					deltaSize.X = avSize;
					deltaSize.Y = avSize;
				}
			}
			for (var i = 0; i < points.Count; i++) {
				var deltaPos = basisInversed * initialPointsPosition[i] * deltaSize * basis - Vector2.Zero * basis;
				Core.Operations.SetAnimableProperty.Perform(points[i], nameof(PointObject.Position), initialPointsPosition[i] + deltaPos);
			}
		}
	}
}
