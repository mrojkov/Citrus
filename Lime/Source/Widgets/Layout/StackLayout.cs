using System;
using System.Linq;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class StackLayout : Layout, ILayout
	{
		[YuzuMember]
		public bool HorizontallySizeable { get; set; }

		[YuzuMember]
		public bool VerticallySizeable { get; set; }

		public StackLayout()
		{
			DebugRectangles = new List<Rectangle>();
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
			var minSize = new Vector2(widgets.Max(i => i.EffectiveMinSize.X), widgets.Max(i => i.EffectiveMinSize.Y)) + widget.Padding;
			var maxSize = new Vector2(widgets.Max(i => i.EffectiveMaxSize.X), widgets.Max(i => i.EffectiveMaxSize.Y)) + widget.Padding;
			if (HorizontallySizeable) {
				minSize.X = 0;
				maxSize.X = float.PositiveInfinity;
			}
			if (VerticallySizeable) {
				minSize.Y = 0;
				maxSize.Y = float.PositiveInfinity;
			}
			widget.MeasuredMinSize = minSize;
			widget.MeasuredMaxSize = maxSize;
		}

		public override void ArrangeChildren(Widget widget)
		{
			DebugRectangles.Clear();
			ArrangementValid = true;
			foreach (var child in GetChildren(widget)) {
				var position = widget.ContentPosition;
				var size = widget.ContentSize;
				var align = EffectiveLayoutCell(child).Alignment;
				if (HorizontallySizeable) {
					position.X = child.X;
					size.X = child.EffectiveMinSize.X;
					align.X = HAlignment.Left;
				}
				if (VerticallySizeable) {
					position.Y = child.Y;
					size.Y = child.EffectiveMinSize.Y;
					align.Y = VAlignment.Top;
				}
				LayoutWidgetWithinCell(child, position, size, align, DebugRectangles);
			}
		}

		public override NodeComponent Clone()
		{
			return (StackLayout)base.Clone();
		}
	}
}
