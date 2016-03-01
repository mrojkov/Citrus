using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class VBoxLayout : CommonLayout, ILayout
	{
		public float Spacing { get; set; }

		public VBoxLayout()
		{
			DebugRectangles = new List<Rectangle>();
		}

		public override void ArrangeChildren(Widget widget)
		{
			ArrangementValid = true;
			var widgets = GetChildren(widget);
			if (widgets.Count == 0) {
				return;
			}
			var constraints = new LinearAllocator.Constraints[widgets.Count];
			int i = 0;
			foreach (var w in widgets) {
				constraints[i++] = new LinearAllocator.Constraints {
					MinSize = w.MinSize.Y,
					MaxSize = w.MaxSize.Y,
					Stretch = (w.LayoutCell ?? LayoutCell.Default).StretchY
				};
			}
			var availableHeight = Math.Max(0, widget.ContentHeight - (widgets.Count - 1) * Spacing);
			var sizes = LinearAllocator.Allocate(availableHeight, constraints, roundSizes: true);
			i = 0;
			DebugRectangles.Clear();
			var position = new Vector2(widget.Padding.Left, widget.Padding.Top);
			foreach (var w in widgets) {
				var size = new Vector2(widget.ContentWidth, sizes[i]);
				LayoutWidgetWithinCell(w, position, size, DebugRectangles);
				position.Y += size.Y + Spacing;
				i++;
			}
		}

		public override void MeasureSizeConstraints(Widget widget)
		{
			ConstraintsValid = true;
			var widgets = GetChildren(widget);
			if (widgets.Count == 0) {
				widget.MinSize = Vector2.Zero;
				widget.MaxSize = Vector2.PositiveInfinity;
				return;
			}
			var minSize = new Vector2(
				widgets.Max(i => i.MinSize.X),
				widgets.Sum(i => i.MinSize.Y)
			);
			var maxSize = new Vector2(
				widgets.Max(i => i.MaxSize.X),
				widgets.Sum(i => i.MaxSize.Y)
			);
			var extraSpace = new Vector2(0, (widgets.Count - 1) * Spacing) + widget.Padding;
			widget.MinSize = minSize + extraSpace;
			widget.MaxSize = maxSize + extraSpace;
		}
	}
}