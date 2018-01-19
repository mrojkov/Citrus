using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ResizeWidgetsProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				Quadrangle hull;
				Vector2 pivot;
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>();
				if (Utils.CalcHullAndPivot(widgets, sv.Scene, out hull, out pivot)) {
					for (int i = 0; i < 4; i++) {
						var a = hull[i];
						if (sv.HitTestResizeControlPoint(a)) {
							Utils.ChangeCursorIfDefault(MouseCursor.SizeNS);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Resize(hull, i * 2, pivot);
							}
						}
						var b = hull[(i + 1) % 4];
						if (sv.HitTestResizeControlPoint((a + b) / 2)) {
							var cursor = (b.X - a.X).Abs() > (b.Y - a.Y).Abs() ? MouseCursor.SizeNS : MouseCursor.SizeWE;
							Utils.ChangeCursorIfDefault(cursor);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Resize(hull, i * 2 + 1, pivot);
							}
						}
					}
				}
				yield return null;
			}
		}

		IEnumerator<object> Resize(Quadrangle hull, int controlPointIndex, Vector2 pivot)
		{
			var cursor = WidgetContext.Current.MouseCursor;
			Document.Current.History.BeginTransaction();
			try {
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
				var mouseStartPos = sv.MousePosition;

				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RevertActiveTransaction();

					Utils.ChangeCursorIfDefault(cursor);
					var proportional = sv.Input.IsKeyPressed(Key.Shift);

					if (sv.Input.IsKeyPressed(Key.Control)) {
						RescaleWidgets(hull, widgets.Count <= 1, pivot, widgets, controlPointIndex, sv.MousePosition, mouseStartPos,
							proportional);
					} else {
						foreach (Widget widget in widgets) {
							ResizeWidget(widget, controlPointIndex, sv.MousePosition, mouseStartPos, proportional);
						}
					}

					yield return null;
				}
			} finally {
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
			}
		}

		private static readonly int[] LookupPivotIndex = {
			4, 5, 6, 7, 0, 1, 2, 3
		};

		private static readonly bool[][] LookupInvolvedAxes = {
			new[] {true, true},
			new[] {false, true},
			new[] {true, true},
			new[] {true, false},
			new[] {true, true},
			new[] {false, true},
			new[] {true, true},
			new[] {true, false},
		};

		readonly Vector2[] directionLookup = {
			new Vector2(-1, -1),
			new Vector2(0, -1),
			new Vector2(1, -1),
			new Vector2(1, 0),
			new Vector2(1, 1),
			new Vector2(0, 1),
			new Vector2(-1, 1),
			new Vector2(-1, 0),
		};

		readonly Vector2[] positionLookup = {
			new Vector2(1, 1),
			new Vector2(0, 1),
			new Vector2(0, 1),
			new Vector2(0, 0),
			new Vector2(0, 0),
			new Vector2(0, 0),
			new Vector2(1, 0),
			new Vector2(1, 0),
		};

		void ResizeWidget(Widget widget, int controlPointIndex, Vector2 curMousePos, Vector2 prevMousePos, bool proportional)
		{
			var mouseDelta = curMousePos - prevMousePos;
			var transform = sv.Scene.CalcTransitionToSpaceOf(widget.ParentWidget);
			var transformedMouseDelta = Vector2.RotateDeg(mouseDelta * transform - Vector2.Zero * transform, -widget.Rotation);
			var deltaSize = transformedMouseDelta * directionLookup[controlPointIndex];
			var deltaPosition = transformedMouseDelta * positionLookup[controlPointIndex];
			var avSize = 0.5f * (deltaSize.X + deltaSize.Y);
			var avPos = 0.5f * (deltaPosition.Y + deltaPosition.X);
			if (proportional) {
				switch (controlPointIndex) {
					case 0:
					case 4:
						deltaSize.X = avSize;
						deltaSize.Y = avSize;
						deltaPosition.X = avPos;
						deltaPosition.Y = avPos;
						break;
					case 2:
						deltaSize.X = avSize;
						deltaSize.Y = avSize;
						deltaPosition.Y = -avSize;
						break;
					case 1:
					case 5:
						deltaSize.X = deltaSize.Y;
						deltaPosition.X = deltaPosition.Y;
						break;
					case 6:
						deltaSize.X = avSize;
						deltaSize.Y = avSize;
						deltaPosition.X = -avSize;
						break;
					case 3:
					case 7:
						deltaSize.Y = deltaSize.X;
						break;
				}
			}

			var size = widget.Size + (deltaSize / widget.Scale).Snap(Vector2.Zero);
			if (float.IsInfinity(size.X)) {
				size.X = size.X.Sign() * Mathf.ZeroTolerance;
			}
			if (float.IsInfinity(size.Y)) {
				size.Y = size.Y.Sign() * Mathf.ZeroTolerance;
			}

			var position = widget.Position +
					Vector2.RotateDeg(deltaPosition + widget.Pivot * deltaSize, widget.Rotation).Snap(Vector2.Zero);
			Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), position);
			Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Size), size);
		}

		void RescaleWidgets(Quadrangle originalHull, bool hullInFirstWidgetSpace, Vector2 hullsPivotPoint, List<Widget> widgets, int controlPointIndex,
			Vector2 curMousePos, Vector2 prevMousePos, bool proportional)
		{
			Utils.ApplyTransformationToWidgetsGroupOobb(
				sv.Scene,
				widgets, hullsPivotPoint, hullInFirstWidgetSpace, curMousePos, prevMousePos,
				(originalVectorInOobbSpace, deformedVectorInOobbSpace) => {
					Vector2 deformationScaleInOobbSpace =
							new Vector2(
								Math.Abs(originalVectorInOobbSpace.X) < Mathf.ZeroTolerance
									? 1
									: deformedVectorInOobbSpace.X / originalVectorInOobbSpace.X,
								Math.Abs(originalVectorInOobbSpace.Y) < Mathf.ZeroTolerance
									? 1
									: deformedVectorInOobbSpace.Y / originalVectorInOobbSpace.Y
							);
					if (proportional) {
						deformationScaleInOobbSpace.X = (deformationScaleInOobbSpace.X + deformationScaleInOobbSpace.Y) / 2;
						deformationScaleInOobbSpace.Y = deformationScaleInOobbSpace.X;
					}

					if (!LookupInvolvedAxes[controlPointIndex][0]) {
						deformationScaleInOobbSpace.X = proportional ? deformationScaleInOobbSpace.Y : 1;
					}
					if (!LookupInvolvedAxes[controlPointIndex][1]) {
						deformationScaleInOobbSpace.Y = proportional ? deformationScaleInOobbSpace.X : 1;
					}

					return Matrix32.Scaling(deformationScaleInOobbSpace);
				}
			);
		}

	}
}