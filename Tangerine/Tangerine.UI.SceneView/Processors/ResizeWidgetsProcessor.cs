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
								yield return Resize(i * 2, pivot);
							}
						}
						var b = hull[(i + 1) % 4];
						if (sv.HitTestResizeControlPoint((a + b) / 2)) {
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
				var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
				var mouseStartPos = sv.MousePosition;
				var startStates = widgets.Select(w => new WidgetState(w)).ToList();

				while (sv.Input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(cursor);
					var proportional = sv.Input.IsKeyPressed(Key.Shift);
					for (int i = 0; i < widgets.Count; i++) {
						startStates[i].SetDataTo(widgets[i]);
						if (sv.Input.IsKeyPressed(Key.Control)) {
							RescaleWidget(widgets[i], controlPointIndex, sv.MousePosition, mouseStartPos, pivot, proportional);
						} else {
							ResizeWidget(widgets[i], controlPointIndex, sv.MousePosition, mouseStartPos, proportional);
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
				scale.Y  = a.Y != 0 ? b.Y / a.Y : 0;
				if (proportional) {
					scale.X = scale.Y;
				}
			}

			var newPivot = sv.Scene.CalcTransitionToSpaceOf(widget) * masterPivot;
			if (float.IsInfinity(newPivot.X) || float.IsNaN(newPivot.X)) {
				newPivot.X = 0;
			}

			if (float.IsInfinity(newPivot.Y) || float.IsNaN(newPivot.Y)) {
				newPivot.Y = 0;
			}
			
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

		private class WidgetState
		{
			Vector2 Position { get; set; }
			Vector2 Scale { get; set; }
			Vector2 Size { get; set; }
			Vector2 Pivot { get; set; }

			public WidgetState(Widget widget)
			{
				Position = widget.Position;
				Scale = widget.Scale;
				Size = widget.Size;
				Pivot = widget.Pivot;
			}

			internal void SetDataTo(Widget widget)
			{
				widget.Position = Position;
				widget.Scale = Scale;
				widget.Size = Size;
				widget.Pivot = Pivot;
			}
		}
	}
}