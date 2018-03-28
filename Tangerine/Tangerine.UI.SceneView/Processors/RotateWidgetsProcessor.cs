using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI.SceneView
{
	public class RotateWidgetsProcessor : ITaskProvider
	{
		private SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				Quadrangle hull;
				Vector2 pivot;
				IEnumerable<Widget> widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>();
				if (Utils.CalcHullAndPivot(widgets, sv.Scene, out hull, out pivot)) {
					for (int i = 0; i < 4; i++) {
						if (sv.HitTestControlPoint(hull[i])) {
							Utils.ChangeCursorIfDefault(Cursors.Rotate);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Rotate(pivot);
							}
						}
					}
				}
				yield return null;
			}
		}

		private IEnumerator<object> Rotate(Vector2 pivot)
		{
			Document.Current.History.BeginTransaction();
			try {
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
				var mouseStartPos = sv.MousePosition;

				List<Tuple<Widget, AccumulativeRotationHelper>> accumulateRotationHelpers =
					widgets.Select(widget =>
						new Tuple<Widget, AccumulativeRotationHelper>(widget, new AccumulativeRotationHelper(widget.Rotation, 0))
					).ToList();
				
				while (sv.Input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(Cursors.Rotate);
					Document.Current.History.RevertActiveTransaction();
					RotateWidgets(pivot, widgets, sv.MousePosition, mouseStartPos, sv.Input.IsKeyPressed(Key.Shift), accumulateRotationHelpers);
					yield return null;
				}
			} finally {
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
			}
		}

		private void RotateWidgets(Vector2 pivotPoint, List<Widget> widgets, Vector2 curMousePos, Vector2 prevMousePos,
			bool snapped, List<Tuple<Widget, AccumulativeRotationHelper>> accumulativeRotationHelpers)
		{
			Utils.ApplyTransformationToWidgetsGroupObb(
				sv.Scene,
				widgets, pivotPoint, false, curMousePos, prevMousePos,
				(originalVectorInObbSpace, deformedVectorInObbSpace) => {

					float rotation = 0;
					if (originalVectorInObbSpace.Length > Mathf.ZeroTolerance &&
						deformedVectorInObbSpace.Length > Mathf.ZeroTolerance) {
						rotation = Mathf.Wrap180(deformedVectorInObbSpace.Atan2Deg - originalVectorInObbSpace.Atan2Deg);
					}

					if (snapped) {
						rotation = Utils.RoundTo(rotation, 15);
					}

					foreach (Tuple<Widget, AccumulativeRotationHelper> tuple in accumulativeRotationHelpers) {
						tuple.Item2.Rotate(rotation);
					}

					return Matrix32.Rotation(rotation * Mathf.DegToRad);
				}
			);

			foreach (Tuple<Widget, AccumulativeRotationHelper> tuple in accumulativeRotationHelpers) {
				SetAnimableProperty.Perform(tuple.Item1, nameof(Widget.Rotation), tuple.Item2.Rotation,
					CoreUserPreferences.Instance.AutoKeyframes);
			}
		}

	}
}
