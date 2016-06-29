using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class ResizeGridCurveViewProcessor : Core.IProcessor
	{
		const float separatorThickness = 10;

		public IEnumerator<object> MainLoop()
		{
			while (true) {
				foreach (var row in Timeline.Instance.Rows) {
					if (!row.Components.Has<CurveRow>()) {
						continue;
					}
					var widget = row.Components.Get<IGridWidget>().Widget;
					if (widget != null && widget.Input.WasMousePressed()) {
						if (HitTestSeparator(widget)) {
							yield return DragSeparatorTask(row);
						}
					}
				}
				yield return null;
			}
		}

		private bool HitTestSeparator(Widget widget)
		{
			var mouse = widget.Input.MousePosition;
			var r = widget.CalcAABBInWindowSpace();
			return Mathf.Abs(mouse.Y - r.Bottom) < separatorThickness / 2 && mouse.X >= r.Left && mouse.X <= r.Right;
		}

		private IEnumerator<object> DragSeparatorTask(Row row)
		{
			var widget = row.Components.Get<IGridWidget>().Widget;
			var cr = row.Components.Get<CurveRow>();
			var input = widget.Input;
			var initialMousePosition = input.MousePosition;
			var initialHeight = cr.State.RowHeight;
			input.CaptureMouse();
			while (input.IsMousePressed()) {
				widget.MinHeight = cr.State.RowHeight = initialHeight + (input.MousePosition.Y - initialMousePosition.Y);
				yield return null;
			}
			input.ReleaseMouse();
		}
	}
}