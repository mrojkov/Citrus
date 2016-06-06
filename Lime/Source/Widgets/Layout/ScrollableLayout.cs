using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class ScrollableLayout : CommonLayout, ILayout
	{
		public ScrollDirection ScrollDirection { get; set; }

		public ScrollableLayout() : this(ScrollDirection.Both) { }

		public ScrollableLayout(ScrollDirection scrollDirection)
		{
			DebugRectangles = new List<Rectangle>();
			ScrollDirection = scrollDirection;
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
			if (ScrollDirection == ScrollDirection.Vertical) {
				var minWidth = widgets.Max(i => i.EffectiveMinSize.X);
				var maxWidth = widgets.Max(i => i.EffectiveMaxSize.X);
				var paddingX = widget.Padding.Left + widget.Padding.Right;
				widget.MeasuredMinSize = new Vector2(minWidth + paddingX, 0);
				widget.MeasuredMaxSize = new Vector2(maxWidth + paddingX, float.PositiveInfinity);
			} else if (ScrollDirection == ScrollDirection.Horizontal) {
				var minHeight = widgets.Max(i => i.EffectiveMinSize.Y);
				var maxHeight = widgets.Max(i => i.EffectiveMaxSize.Y);
				var paddingY = widget.Padding.Top + widget.Padding.Bottom;
				widget.MeasuredMinSize = new Vector2(0, minHeight + paddingY);
				widget.MeasuredMaxSize = new Vector2(float.PositiveInfinity, maxHeight + paddingY);
			} else if (ScrollDirection == ScrollDirection.Both) {
				widget.MeasuredMinSize = Vector2.Zero;
				widget.MeasuredMaxSize = Vector2.PositiveInfinity;
			} else {
				throw new InvalidOperationException();
			}
		}

		public override void ArrangeChildren(Widget widget)
		{
			DebugRectangles.Clear();
			ArrangementValid = true;
			foreach (var child in GetChildren(widget)) {
				LayoutWidgetWithinCell(child, widget.ContentPosition, widget.ContentSize, DebugRectangles);
			}
		}
	}
}