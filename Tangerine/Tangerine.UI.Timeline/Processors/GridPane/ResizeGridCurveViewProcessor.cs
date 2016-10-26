using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class ResizeGridCurveViewProcessor : Core.ITaskProvider
	{
		const float separatorThickness = 10;

		public IEnumerator<object> Loop()
		{
			while (true) {
				foreach (var row in Document.Current.Rows) {
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