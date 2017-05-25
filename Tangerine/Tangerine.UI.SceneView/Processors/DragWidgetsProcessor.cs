using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragWidgetsProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				Quadrangle hull;
				Vector2 pivot;
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>();
				if (Utils.CalcHullAndPivot(widgets, sv.Scene, out hull, out pivot) && sv.HitTestControlPoint(pivot)) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
						yield return Drag();
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
			Document.Current.History.BeginTransaction();
			try {
				var iniMousePos = sv.MousePosition;
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
				var transform = sv.Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
				var dragDirection = DragDirection.Any;
				var positions = widgets.Select(i => i.Position).ToList();
				while (sv.Input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					var curMousePos = sv.MousePosition;
					var shiftPressed = sv.Input.IsKeyPressed(Key.Shift);
					if (shiftPressed && dragDirection != DragDirection.Any) {
						if (dragDirection == DragDirection.Horizontal) {
							curMousePos.Y = iniMousePos.Y;
						} else if (dragDirection == DragDirection.Vertical) {
							curMousePos.X = iniMousePos.X;
						}
					}
					var dragDelta = curMousePos * transform - iniMousePos * transform;
					if (shiftPressed && dragDirection == DragDirection.Any && (curMousePos - iniMousePos).Length > 5 / sv.Scene.Scale.X) {
						var d = curMousePos - iniMousePos;
						dragDirection = d.X.Abs() > d.Y.Abs() ? DragDirection.Horizontal : DragDirection.Vertical;
					}
					dragDelta = dragDelta.Snap(Vector2.Zero);
					if (dragDelta != Vector2.Zero) {
						for (int i = 0; i < widgets.Count; i++) {
							Core.Operations.SetAnimableProperty.Perform(widgets[i], nameof(Widget.Position), positions[i] + dragDelta);
						}
					}
					yield return null;
				}
			} finally {
				sv.Input.ReleaseMouse();
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.EndTransaction();
			}
		}
	}
}