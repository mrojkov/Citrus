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
		private Quadrangle hullNormalized;
		private Vector2 initialMousePosition;

		public IEnumerator<object> Task()
		{
			while (true) {
				if (!SceneView.Instance.InputArea.IsMouseOverThisOrDescendant()) {
					yield return null;
					continue;
				}
				var points = Document.Current.SelectedNodes().Editable().OfType<PointObject>().ToList();
				if (points.Count > 1) {
					Utils.CalcHullAndPivot(points, out var hull, out _);
					hull = hull.Transform(Document.Current.Container.AsWidget.LocalToWorldTransform.CalcInversed());
					var hullSize = hull.V3 - hull.V1;
					hullNormalized = hull * Matrix32.Scaling(Vector2.One / Document.Current.Container.AsWidget.Size);
					var expandedHullInSceneCoords = PointObjectsPresenter.ExpandAndTranslateToSpaceOf(hull, Document.Current.Container.AsWidget, sv) *
						sv.Frame.CalcTransitionToSpaceOf(sv.Scene);
					for (int i = 0; i < 4; i++) {
						if (Mathf.Abs(hullSize.X) > Mathf.ZeroTolerance && Mathf.Abs(hullSize.Y) > Mathf.ZeroTolerance) {
							if (sv.HitTestResizeControlPoint(expandedHullInSceneCoords[i])) {
								Utils.ChangeCursorIfDefault(MouseCursor.SizeNS);
								if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
									yield return Rescale(i * 2, MouseCursor.SizeNS, points);
								}
							}
						}
					}
					for (int i = 0; i < 4; i++) {
						if (Mathf.Abs(hullSize.X) < Mathf.ZeroTolerance && i % 2 == 1 ||
						    Mathf.Abs(hullSize.Y) < Mathf.ZeroTolerance && i % 2 == 0
						) {
							continue;
						}
						var a = expandedHullInSceneCoords[i];
						var b = expandedHullInSceneCoords[(i + 1) % 4];
						if (sv.HitTestResizeControlPoint((a + b) / 2)) {
							var cursor = MouseCursor.Default;
							if (Mathf.Abs(hullSize.X) < Mathf.ZeroTolerance) {
								cursor = MouseCursor.SizeNS;
							} else if (Mathf.Abs(hullSize.Y) < Mathf.ZeroTolerance) {
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
			var t = Document.Current.Container.AsWidget.LocalToWorldTransform.CalcInversed();
			using (Document.Current.History.BeginTransaction()) {
				Utils.ChangeCursorIfDefault(cursor);
				initialMousePosition = sv.MousePosition;
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();
					Utils.ChangeCursorIfDefault(cursor);
					RescaleHelper(
						points,
						controlPointIndex,
						sv.Input.IsKeyPressed(Key.Shift),
						sv.Input.IsKeyPressed(Key.Control));
					yield return null;
				}
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.CommitTransaction();
			}
		}

		void RescaleHelper(List<PointObject> points, int controlPointIndex, bool proportional, bool centerProportional)
		{
			var t = Document.Current.Container.AsWidget.LocalToWorldTransform.CalcInversed();
			t.T = Vector2.Zero;
			var transformedMouseDelta = (sv.MousePosition - initialMousePosition) * t / Document.Current.Container.AsWidget.Size;
			Vector2 origin;
			var idx = (controlPointIndex + 4) % 8 / 2;
			var next = (idx + 1) % 4;
			var prev = (idx + 3) % 4;
			var axisX = hullNormalized[next] - hullNormalized[idx];
			var axisY = hullNormalized[prev] - hullNormalized[idx];
			if (controlPointIndex == 7 || controlPointIndex == 3 || controlPointIndex == 1 || controlPointIndex == 5) {
				origin = (hullNormalized[next] + hullNormalized[idx]) / 2;
			} else {
				origin = hullNormalized[idx];
			}
			axisX.Snap(Vector2.Zero);
			axisY.Snap(Vector2.Zero);
			if (axisX == Vector2.Zero) {
				axisX = new Vector2(-axisY.Y, axisY.X);
			}
			if (axisY == Vector2.Zero) {
				axisY = new Vector2(-axisX.Y, axisX.X);
			}
			var basis = new Matrix32(axisX, axisY, centerProportional ? (hullNormalized.V1 + hullNormalized.V3) / 2 : origin);
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
				var deltaPos = basisInversed * points[i].Position * deltaSize * basis - Vector2.Zero * basis;
				Core.Operations.SetAnimableProperty.Perform(points[i], nameof(PointObject.Position), points[i].Position + deltaPos, CoreUserPreferences.Instance.AutoKeyframes);
			}
		}
	}
}
