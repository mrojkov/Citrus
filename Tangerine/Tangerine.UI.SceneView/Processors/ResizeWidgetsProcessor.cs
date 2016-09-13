using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ResizeWidgetsProcessor : IProcessor
	{
		public IEnumerator<object> Loop()
		{
			while (true) {
				if (SceneView.Instance.Input.WasMousePressed()) {
					var widgets = Utils.UnlockedWidgets();
					if (widgets.Count > 0) {
						Quadrangle hull;
						Vector2 pivot;
						Utils.CalcHullAndPivot(widgets, SceneView.Instance.Scene, out hull, out pivot);
						for (int i = 0; i < 4; i++) {
							if (HitTestControlPoint(hull[i])) {
								yield return Resize(i * 2);
							}
							if (HitTestControlPoint((hull[(i + 1) % 4] + hull[i]) / 2)) {
								yield return Resize(i * 2 + 1);
							}
						}
					}
				}
				yield return null;
			}
		}

		bool HitTestControlPoint(Vector2 controlPoint)
		{
			return (controlPoint - SceneView.Instance.MousePosition).Length < 6;
		}

		IEnumerator<object> Resize(int controlPointIndex)
		{
			var sv = SceneView.Instance;
			sv.Input.CaptureMouse();
			sv.Input.ConsumeKey(Key.Mouse0);
			var widgets = Utils.UnlockedWidgets();
			var mousePos = sv.MousePosition;
			while (sv.Input.IsMousePressed()) {
				var mouseDelta = sv.MousePosition - mousePos;
				var proportional = sv.Input.IsKeyPressed(Key.LShift);
				foreach (var widget in widgets) {
					ProcessWidget(widget, controlPointIndex, mouseDelta, proportional);
				}
				mousePos = sv.MousePosition;
				yield return null;
			}
			sv.Input.ReleaseMouse();
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

		void ProcessWidget(Widget widget, int controlPointIndex, Vector2 mouseDelta, bool proportional)
		{
			var rotatedMouseDelta = Vector2.RotateDeg(mouseDelta, -widget.Rotation);
			var deltaSize = rotatedMouseDelta * directionLookup[controlPointIndex];
			var deltaPosition = rotatedMouseDelta * positionLookup[controlPointIndex];
			if (proportional) {
				if(controlPointIndex == 1 || controlPointIndex == 5) {
					deltaSize.Y = deltaSize.X;
					deltaPosition.Y = deltaPosition.X;
				} else {
					deltaSize.X = deltaSize.Y;
					deltaPosition.X = deltaPosition.Y;
				}
			}
			var size = widget.Size + deltaSize / widget.Scale;
			var position = widget.Position + Vector2.RotateDeg(deltaPosition + widget.Pivot * deltaSize, widget.Rotation);
			Core.Operations.SetAnimableProperty.Perform(widget, "Position", position);
			Core.Operations.SetAnimableProperty.Perform(widget, "Size", size);
		}
	}
}