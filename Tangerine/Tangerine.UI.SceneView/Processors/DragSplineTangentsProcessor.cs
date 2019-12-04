using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragSplineTangentsProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				if (!SceneView.Instance.InputArea.IsMouseOverThisOrDescendant()) {
					yield return null;
					continue;
				}
				var points = Document.Current.SelectedNodes().Editable().OfType<SplinePoint>();
				foreach (var point in points) {
					for (int i = 0; i < 2; i++) {
						var p = CalcTangentKnobPosition(point, i);
						if (sv.HitTestControlPoint(p, 5)) {
							Utils.ChangeCursorIfDefault(MouseCursor.Hand);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Drag(point, i);
							}
						}
					}
				}
				yield return null;
			}
		}

		Vector2 CalcTangentKnobPosition(SplinePoint point, int index)
		{
			var matrix = (Document.Current.Container as Widget).LocalToWorldTransform;
			var delta = (index == 0 ? -1 : 1) * SplinePointPresenter.TangentWeightRatio * point.TangentWeight * Vector2.CosSin(point.TangentAngle * Mathf.DegToRad);
			return matrix * (point.TransformedPosition + delta);
		}

		IEnumerator<object> Drag(SplinePoint point, int index)
		{
			using (Document.Current.History.BeginTransaction()) {
				var iniMousePos = sv.MousePosition;
				var matrix = (Document.Current.Container as Widget).LocalToWorldTransform.CalcInversed();
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var curMousePos = sv.MousePosition;
					if ((curMousePos - iniMousePos).Snap(Vector2.Zero) != Vector2.Zero) {
						var p = matrix * curMousePos;
						var o = point.TransformedPosition;
						var angle = (index == 0 ? o - p : p - o).Atan2Deg;
						var weight = (p - o).Length / SplinePointPresenter.TangentWeightRatio;
						if (!Window.Current.Input.IsKeyPressed(Key.Shift)) {
							Core.Operations.SetAnimableProperty.Perform(point, nameof(SplinePoint.TangentAngle), angle, CoreUserPreferences.Instance.AutoKeyframes);
						}
						if (!Window.Current.Input.IsKeyPressed(Key.Control)) {
							Core.Operations.SetAnimableProperty.Perform(point, nameof(SplinePoint.TangentWeight), weight, CoreUserPreferences.Instance.AutoKeyframes);
						}
					}
					yield return null;
				}
				Document.Current.History.CommitTransaction();
			}
		}
	}
}
