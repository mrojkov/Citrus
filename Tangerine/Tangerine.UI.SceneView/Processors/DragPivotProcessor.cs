using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DragPivotProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>();
				if (
					SceneView.Instance.InputArea.IsMouseOverThisOrDescendant() &&
					sv.Input.IsKeyPressed(Key.Control) &&
					Utils.CalcHullAndPivot(widgets, out var hull, out var pivot) &&
					sv.HitTestControlPoint(pivot))
				{
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
						yield return sv.Input.IsKeyPressed(Key.Alt) ? DragShared(hull) : Drag();
					}
				}
				yield return null;
			}
		}

		enum DragDirection
		{
			Any, Horizontal, Vertical
		}

		IEnumerator<object> DragShared(Quadrangle hull)
		{
			var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
			using (Document.Current.History.BeginTransaction()) {
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();
					var curMousePos = SnapMousePosToSpecialPoints(hull, sv.MousePosition, Vector2.Zero);
					foreach (var widget in widgets) {
						var transform = widget.LocalToWorldTransform.CalcInversed();
						var newPivot = curMousePos * transform / widget.Size;
						Core.Operations.SetAnimableProperty.Perform(
							widget, nameof(Widget.Position),
							curMousePos * widget.ParentWidget.LocalToWorldTransform.CalcInversed(),
							CoreUserPreferences.Instance.AutoKeyframes
						);
						Core.Operations.SetAnimableProperty.Perform(
							widget, nameof(Widget.Pivot), newPivot,
							CoreUserPreferences.Instance.AutoKeyframes
						);
					}
					yield return null;
				}
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.CommitTransaction();
			}
		}

		IEnumerator<object> Drag()
		{
			var iniMousePos = sv.MousePosition;
			var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
			using (Document.Current.History.BeginTransaction()) {
				var dragDirection = DragDirection.Any;
				Utils.CalcHullAndPivot(widgets, out var hull, out var iniPivot);
				while (sv.Input.IsMousePressed()) {
					Document.Current.History.RollbackTransaction();
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
					curMousePos = SnapMousePosToSpecialPoints(hull, curMousePos, iniMousePos - iniPivot);
					if (
						shiftPressed &&
						dragDirection == DragDirection.Any &&
						(curMousePos - iniMousePos).Length > 5
					) {
						var d = curMousePos - iniMousePos;
						dragDirection =
							d.X.Abs() > d.Y.Abs() ?
							DragDirection.Horizontal :
							DragDirection.Vertical;
					}
					foreach (var widget in widgets) {
						var transform = widget.LocalToWorldTransform.CalcInversed();
						var dragDelta = curMousePos * transform - iniMousePos * transform;
						var deltaPivot = dragDelta / widget.Size;
						var deltaPos = Vector2.RotateDeg(dragDelta * widget.Scale, widget.Rotation);
						deltaPivot = deltaPivot.Snap(Vector2.Zero);
						if (deltaPivot != Vector2.Zero) {
							Core.Operations.SetAnimableProperty.Perform(
								widget, nameof(Widget.Pivot),
								widget.Pivot + deltaPivot,
								CoreUserPreferences.Instance.AutoKeyframes
							);
							Core.Operations.SetAnimableProperty.Perform(
								widget, nameof(Widget.Position),
								widget.Position + deltaPos.Snap(Vector2.Zero),
								CoreUserPreferences.Instance.AutoKeyframes
							);
						}
					}
					yield return null;
				}
				sv.Input.ConsumeKey(Key.Mouse0);
				Document.Current.History.CommitTransaction();
			}
		}

		Vector2 SnapMousePosToSpecialPoints(Quadrangle hull, Vector2 mousePos, Vector2 correction)
		{
			var md = float.MaxValue;
			var mp = Vector2.Zero;
			foreach (var p in GetSpecialPoints(hull)) {
				var d = (mousePos - p).Length;
				if (d < md) {
					mp = p;
					md = d;
				}
			}
			const float SnapDistance = 12;
			var r = Mathf.Min(SnapDistance / sv.Scene.Scale.X, (hull[0] - hull[2]).Length * 0.25f);
			if (md < r) {
				return mp + correction;
			}
			return mousePos;
		}

		IEnumerable<Vector2> GetSpecialPoints(Quadrangle hull)
		{
			for (int i = 0; i < 4; i++) {
				yield return hull[i];
				yield return (hull[i] + hull[(i + 1) % 4]) / 2;
			}
			yield return (hull[0] + hull[2]) / 2;
		}
	}
}
