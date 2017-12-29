using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class VBoxLayout : CommonLayout, ILayout
	{
		public float Spacing { get; set; }
		public LayoutCell DefaultCell { get; set; }

		public VBoxLayout()
		{
			DefaultCell = new LayoutCell();
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
					MinSize = w.EffectiveMinSize.Y,
					MaxSize = w.EffectiveMaxSize.Y,
					Stretch = (w.LayoutCell ?? DefaultCell).StretchY
				};
			}
			var availableHeight = Math.Max(0, widget.ContentHeight - (widgets.Count - 1) * Spacing);
			var sizes = LinearAllocator.Allocate(availableHeight, constraints, roundSizes: true);
			i = 0;
			DebugRectangles.Clear();
			var position = widget.Padding.LeftTop;
			foreach (var child in widgets) {
				var size = new Vector2(widget.ContentWidth, sizes[i]);
				var align = (child.LayoutCell ?? DefaultCell).Alignment;
				LayoutWidgetWithinCell(child, position, size, align, DebugRectangles);
				position.Y += size.Y + Spacing;
				i++;
			}
		}

		public override void MeasureSizeConstraints(Widget widget)
		{
			ConstraintsValid = true;
			var widgets = GetChildren(widget);
			if (widgets.Count == 0) {
				widget.MeasuredMinSize = Vector2.Zero;
				widget.MeasuredMaxSize = Vector2.PositiveInfinity;
				return;
			}
			var minSize = new Vector2(
				widgets.Max(i => i.EffectiveMinSize.X),
				widgets.Sum(i => i.EffectiveMinSize.Y));
			var maxSize = new Vector2(
				widgets.Max(i => i.EffectiveMaxSize.X),
				widgets.Sum(i => i.EffectiveMaxSize.Y));
			var extraSpace = new Vector2(0, (widgets.Count - 1) * Spacing) + widget.Padding;
			widget.MeasuredMinSize = minSize + extraSpace;
			widget.MeasuredMaxSize = maxSize + extraSpace;
		}
	}
}