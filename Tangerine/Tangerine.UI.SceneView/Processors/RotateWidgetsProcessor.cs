using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

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

				while (sv.Input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(Cursors.Rotate);

					Document.Current.History.RevertActiveTransaction();
					
					RotateWidgets(pivot, widgets, sv.MousePosition, mouseStartPos, sv.Input.IsKeyPressed(Key.Shift));

					yield return null;
				}
			} finally {
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
			}
		}

		private void RotateWidgets(Vector2 pivotPoint, List<Widget> widgets, Vector2 curMousePos, Vector2 prevMousePos,
			bool discret)
		{
			Utils.ApplyTransformationToWidgetsGroupOobb(
				sv.Scene,
				widgets, pivotPoint, false, curMousePos, prevMousePos,
				(originalVectorInOobbSpace, deformedVectorInOobbSpace) => {

					float rotation = 0;
					if (originalVectorInOobbSpace.Length > Mathf.ZeroTolerance &&
						deformedVectorInOobbSpace.Length > Mathf.ZeroTolerance) {
						rotation = Mathf.Wrap180(deformedVectorInOobbSpace.Atan2Deg - originalVectorInOobbSpace.Atan2Deg);
					}

					if (discret) {
						rotation = Utils.RoundTo(rotation, 15);
					}
					return Matrix32.Rotation(rotation * Mathf.DegToRad);
				}
			);
		}

	}
}
