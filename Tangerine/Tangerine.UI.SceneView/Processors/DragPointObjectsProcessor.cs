using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragPointObjectsProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				Rectangle aabb;
				var pobjects = Document.Current.SelectedNodes().Editable().OfType<PointObject>();
				if (
					Utils.CalcAABB(pobjects, sv.Scene, out aabb) &&
					sv.HitTestControlPoint(aabb.Center))
				{
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
						yield return Drag(pobjects.ToList());
					}
				}
				yield return null;
			}
		}

		enum DragDirection
		{
			Any, Horizontal, Vertical
		}

		IEnumerator<object> Drag(List<PointObject> pobjects)
		{
			sv.Input.CaptureMouse();
			Document.Current.History.BeginTransaction();
			try {
				var iniMousePos = sv.MousePosition;
				var transform = sv.Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
				var dragDirection = DragDirection.Any;
				var positions = pobjects.Select(i => i.TransformedPosition).ToList();
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
						for (int i = 0; i < pobjects.Count; i++) {
							SetPosition(pobjects[i], positions[i] + dragDelta);
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

		public static void SetPosition(PointObject po, Vector2 value)
		{
			if (po.Parent?.AsWidget != null) {
				var parentSize = po.Parent.AsWidget.Size;
				if (parentSize.X != 0 && parentSize.Y != 0) {
					var p = (value - po.Offset) / parentSize;
					Core.Operations.SetAnimableProperty.Perform(po, nameof(PointObject.Position), p);
				}
			}
		}
	}
}