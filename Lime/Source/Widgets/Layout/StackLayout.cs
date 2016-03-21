using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class StackLayout : CommonLayout, ILayout
	{
		public StackLayout()
		{
			DebugRectangles = new List<Rectangle>();
		}

		public override void MeasureSizeConstraints(Widget widget)
		{
			var widgets = GetChildren(widget);
			if (widgets.Count == 0) {
				widget.MinSize = Vector2.Zero;
				widget.MaxSize = Vector2.PositiveInfinity;
				return;
			}
			var minSize = new Vector2(widgets.Max(i => i.MinSize.X), widgets.Max(i => i.MinSize.Y));
			var maxSize = new Vector2(widgets.Max(i => i.MaxSize.X), widgets.Max(i => i.MaxSize.Y));
			widget.MinSize = minSize + widget.Padding;
			widget.MaxSize = maxSize + widget.Padding;
		}

		public override void ArrangeChildren(Widget widget)
		{
			ArrangementValid = true;
			var widgets = GetChildren(widget);
			if (widgets.Count == 0) {
				return;
			}
			DebugRectangles.Clear();
			foreach (var w in widgets) {
				LayoutWidgetWithinCell(w, widget.ContentPosition, widget.ContentSize, DebugRectangles);
			}
		}
	}	
}