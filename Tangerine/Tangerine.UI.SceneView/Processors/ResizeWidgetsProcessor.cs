using Lime;
using System;
using System.Linq;
using System.Collections.Generic;
using Tangerine.Core;
using Tangerine.Core.Operations;
using Tangerine.UI.SceneView.WidgetTransforms;

namespace Tangerine.UI.SceneView
{
	public class ResizeWidgetsProcessor : ITaskProvider
	{
		private static SceneView SceneView => SceneView.Instance;

		private static readonly int[] lookupPivotIndex = {
			4, 5, 6, 7, 0, 1, 2, 3
		};
		private static readonly bool[][] lookupInvolvedAxes = {
			new[] {true, true},
			new[] {false, true},
			new[] {true, true},
			new[] {true, false},
			new[] {true, true},
			new[] {false, true},
			new[] {true, true},
			new[] {true, false},
		};

		private readonly Dictionary<Widget, Vector2> childPositions = new Dictionary<Widget, Vector2>();

		public IEnumerator<object> Task()
		{
			while (true) {
				if (!SceneView.Instance.InputArea.IsMouseOverThisOrDescendant()) {
					yield return null;
					continue;
				}
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>();
				if (Utils.CalcHullAndPivot(widgets, out var hull, out var pivot)) {
					for (var i = 0; i < 4; i++) {
						var a = hull[i];
						if (SceneView.HitTestResizeControlPoint(a)) {
							var cursor = i % 2 == 0 ? MouseCursor.SizeNWSE : MouseCursor.SizeNESW;
							Utils.ChangeCursorIfDefault(cursor);
							if (SceneView.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Resize(hull, i * 2, pivot);
							}
						}
						var b = hull[(i + 1) % 4];
						if (SceneView.HitTestResizeControlPoint((a + b) / 2)) {
							var cursor = (b.X - a.X).Abs() > (b.Y - a.Y).Abs() ? MouseCursor.SizeNS : MouseCursor.SizeWE;
							Utils.ChangeCursorIfDefault(cursor);
							if (SceneView.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Resize(hull, i * 2 + 1, pivot);
							}
						}
					}
				}
				yield return null;
			}
		}

		private IEnumerator<object> Resize(Quadrangle hull, int controlPointIndex, Vector2 pivot)
		{
			var cursor = WidgetContext.Current.MouseCursor;
			using (Document.Current.History.BeginTransaction()) {
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
				var mouseStartPos = SceneView.MousePosition;
				while (SceneView.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();
					Matrix32 transform = Matrix32.Identity;
					Utils.ChangeCursorIfDefault(cursor);
					var proportional = SceneView.Input.IsKeyPressed(Key.Shift);
					var isChangingScale = SceneView.Input.IsKeyPressed(Key.Control);
					var areChildrenFreezed =
						SceneView.Input.IsKeyPressed(Key.Z) &&
						!isChangingScale &&
						widgets.Count == 1;
					if (areChildrenFreezed) {
						transform = widgets[0].CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
					}
					var pivotPoint =
						isChangingScale ?
						(widgets.Count <= 1 ? (Vector2?)null : pivot) :
						hull[lookupPivotIndex[controlPointIndex] / 2];
					RescaleWidgets(
						widgets.Count <= 1,
						pivotPoint,
						widgets,
						controlPointIndex,
						SceneView.MousePosition,
						mouseStartPos,
						proportional,
						!isChangingScale
					);
					if (areChildrenFreezed) {
						transform *= Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(widgets[0]);
						RestoreChildrenPositions(widgets[0], transform);
					}
					yield return null;
				}
				SceneView.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.CommitTransaction();
			}
		}

		private static void RestoreChildrenPositions(Widget widget, Matrix32 transform)
		{
			foreach (var child in widget.Nodes.OfType<Widget>()) {
				var newPosition = transform.TransformVector(child.Position);
				SetProperty.Perform(child, nameof(Widget.Position), newPosition);
				if (child.Animators.TryFind(nameof(Widget.Position), out var animator)) {
					foreach (var key in animator.ReadonlyKeys.ToList()) {
						var newKey = key.Clone();
						newKey.Value = transform.TransformVector((Vector2)key.Value);
						SetKeyframe.Perform(animator, newKey);
					}
				}
			}
		}

		private static void RescaleWidgets(bool hullInFirstWidgetSpace, Vector2? pivotPoint, List<Widget> widgets, int controlPointIndex,
			Vector2 curMousePos, Vector2 prevMousePos, bool proportional, bool convertScaleToSize)
		{
			WidgetTransformsHelper.ApplyTransformationToWidgetsGroupObb(
				SceneView.Scene,
				widgets, pivotPoint, hullInFirstWidgetSpace, curMousePos, prevMousePos,
				convertScaleToSize,
				(originalVectorInObbSpace, deformedVectorInObbSpace) => {
					var deformationScaleInObbSpace = new Vector2d(
						Math.Abs(originalVectorInObbSpace.X) < Mathf.ZeroTolerance ? 1 : deformedVectorInObbSpace.X / originalVectorInObbSpace.X,
						Math.Abs(originalVectorInObbSpace.Y) < Mathf.ZeroTolerance ? 1 : deformedVectorInObbSpace.Y / originalVectorInObbSpace.Y
					);
					if (!lookupInvolvedAxes[controlPointIndex][0]) {
						deformationScaleInObbSpace.X = proportional ? deformationScaleInObbSpace.Y : 1;
					} else if (!lookupInvolvedAxes[controlPointIndex][1]) {
						deformationScaleInObbSpace.Y = proportional ? deformationScaleInObbSpace.X : 1;
					} else if (proportional) {
						deformationScaleInObbSpace.X = (deformationScaleInObbSpace.X + deformationScaleInObbSpace.Y) / 2;
						deformationScaleInObbSpace.Y = deformationScaleInObbSpace.X;
					}
					return new Transform2d(Vector2d.Zero, deformationScaleInObbSpace, 0);
				}
			);
		}

	}
}
