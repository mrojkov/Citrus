using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public class HBoxLayout : CommonLayout, ILayout
	{
		public float Spacing { get; set; }

		public HBoxLayout()
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
					MinSize = w.MinSize.X,
					MaxSize = w.MaxSize.X,
					Stretch = (w.LayoutCell ?? LayoutCell.Default).StretchX
				};
			}
			var availableWidth = Math.Max(0, widget.ContentWidth - (widgets.Count - 1) * Spacing);
			var sizes = LinearAllocator.Allocate(availableWidth, constraints, roundSizes: true);
			i = 0;
			DebugRectangles.Clear();
			var position = new Vector2(widget.Padding.Left, widget.Padding.Top);
			foreach (var w in widgets) {
				var size = new Vector2(sizes[i], widget.ContentHeight);
				LayoutWidgetWithinCell(w, position, size, DebugRectangles);
				position.X += size.X + Spacing;
				i++;
			}
		}

		public override void MeasureSizeConstraints(Widget widget)
		{
			ConstraintsValid = true;
			var widgets = GetChildren(widget);
			var minSize = new Vector2(
				widgets.Sum(i => i.MinSize.X),
				widgets.Max(i => i.MinSize.Y)
			);
			var maxSize = new Vector2(
				widgets.Sum(i => i.MaxSize.X),
				widgets.Max(i => i.MaxSize.Y)
			);
			var extraSpace = new Vector2((widgets.Count - 1) * Spacing, 0) + widget.Padding;
			widget.MinSize = minSize + extraSpace;
			widget.MaxSize = maxSize + extraSpace;
		}
	}
}