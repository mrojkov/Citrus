using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragWidgetsProcessor : IProcessor
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				if (sv.Input.WasMousePressed()) {
					var widgets = Utils.UnlockedWidgets();
					if (widgets.Count > 0) {
						Quadrangle hull;
						Vector2 pivot;
						Utils.CalcHullAndPivot(widgets, sv.Scene, out hull, out pivot);
						if ((pivot - sv.MousePosition).Length < 10) {
							sv.Input.ConsumeKey(Key.Mouse0);
							yield return Drag();
						}
					}
				}
				yield return null;
			}
		}

		enum DragDirection
		{
			Any, Horizontal, Vertical
		}

		IEnumerator<object> Drag()
		{
			sv.Input.CaptureMouse();
			var initialMousePos = sv.MousePosition;
			var widgets = Utils.UnlockedWidgets();
			var transform = Document.Current.Container.AsWidget.CalcTransitionToSpaceOf(sv.Scene);
			var invTransform = transform.CalcInversed();
			var dragDirection = DragDirection.Any;
			var positions = widgets.Select(i => i.Position).ToList();
			while (sv.Input.IsMousePressed()) {
				var curMousePos = sv.MousePosition;
				var shiftPressed = sv.Input.IsKeyPressed(Key.LShift);
				if (shiftPressed && dragDirection != DragDirection.Any) {
					if (dragDirection == DragDirection.Horizontal) {
						curMousePos.Y = initialMousePos.Y;
					} else if (dragDirection == DragDirection.Vertical) {
						curMousePos.X = initialMousePos.X;
					}
				}
				var dragDelta = curMousePos * invTransform - initialMousePos * invTransform;
				if (shiftPressed && dragDirection == DragDirection.Any && (curMousePos - initialMousePos).Length > 5) {
					var d = curMousePos - initialMousePos;
					dragDirection = d.X.Abs() > d.Y.Abs() ? DragDirection.Horizontal : DragDirection.Vertical;
				}
				for (int i = 0; i < widgets.Count; i++) {
					Core.Operations.SetAnimableProperty.Perform(widgets[i], "Position", positions[i] + dragDelta.Snap(Vector2.Zero));
				}
				yield return null;
			}
			sv.Input.ReleaseMouse();
		}
	}
}