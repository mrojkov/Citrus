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
						if (sv.HitTestControlPoint(a, 6)) {
							Utils.ChangeCursorIfDefault(MouseCursor.SizeNS);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Resize(i * 2, pivot);
							}
						}
						var b = hull[(i + 1) % 4];
						if (sv.HitTestControlPoint((a + b) / 2, 6)) {
							var cursor = (b.X - a.X).Abs() > (b.Y - a.Y).Abs() ? MouseCursor.SizeNS : MouseCursor.SizeWE;
							Utils.ChangeCursorIfDefault(cursor);
							if (sv.Input.ConsumeKeyPress(Key.Mouse0)) {
								yield return Resize(i * 2 + 1, pivot);
							}
						}
					}
				}
				yield return null;
			}
		}

		IEnumerator<object> Resize(int controlPointIndex, Vector2 pivot)
		{
			var cursor = WidgetContext.Current.MouseCursor;
			sv.Input.CaptureMouse();
			Document.Current.History.BeginTransaction();
			try {
				var widgets = Core.Document.Current.SelectedNodes().Editable().OfType<Widget>();
				var mousePos = sv.MousePosition;
				while (sv.Input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(cursor);
					var proportional = sv.Input.IsKeyPressed(Key.Shift);
					foreach (var widget in widgets) {
						if (sv.MousePosition != mousePos) {
							if (sv.Input.IsKeyPressed(Key.Control)) {
								RescaleWidget(widget, controlPointIndex, sv.MousePosition, mousePos, pivot, proportional);
							} else {
								ResizeWidget(widget, controlPointIndex, sv.MousePosition, mousePos, proportional);
							}
						}
					}
					mousePos = sv.MousePosition;
					yield return null;
				}
			} finally {
				sv.Input.ReleaseMouse();
				Document.Current.History.EndTransaction();
			}
		}

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
			if (proportional) {
				if(controlPointIndex == 1 || controlPointIndex == 5) {
					deltaSize.Y = deltaSize.X;
					deltaPosition.Y = deltaPosition.X;
				} else {
					deltaSize.X = deltaSize.Y;
					deltaPosition.X = deltaPosition.Y;
				}
			}
			var size = widget.Size + (deltaSize / widget.Scale).Snap(Vector2.Zero);
			var position = widget.Position + Vector2.RotateDeg(deltaPosition + widget.Pivot * deltaSize, widget.Rotation).Snap(Vector2.Zero);
			Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), position);
			Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Size), size);
		}

		void RescaleWidget(Widget widget, int controlPointIndex, Vector2 curMousePos, Vector2 prevMousePos, Vector2 masterPivot, bool proportional)
		{
			var transform = sv.Scene.CalcTransitionToSpaceOf(widget.ParentWidget);
			var a = Vector2.RotateDeg(transform * prevMousePos - transform * masterPivot, -widget.Rotation);
			var b = Vector2.RotateDeg(transform * curMousePos - transform * masterPivot, -widget.Rotation);
			var scale = Vector2.One;
			if (directionLookup[controlPointIndex].X != 0) {
				scale.X = b.X / a.X;
				if (proportional) {
					scale.Y = scale.X;
				}
			}
			if (directionLookup[controlPointIndex].Y != 0) {
				scale.Y = b.Y / a.Y;
				if (proportional) {
					scale.X = scale.Y;
				}
			}
			var newPivot = sv.Scene.CalcTransitionToSpaceOf(widget) * masterPivot;
			newPivot.X /= widget.Width;
			newPivot.Y /= widget.Height;
			var scaledSize = widget.Size * widget.Scale;
			var deltaPos = Vector2.RotateDeg((newPivot - widget.Pivot) * scaledSize, widget.Rotation);
			var newPos = widget.Position + deltaPos;
			var newScale = widget.Scale * scale;
			Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Scale), newScale);
			Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Pivot), newPivot);
			Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), newPos);
		}
	}
}