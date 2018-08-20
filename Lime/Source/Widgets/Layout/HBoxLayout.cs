using System;
using System.Collections.Generic;
using System.Linq;
using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class HBoxLayout : Layout, ILayout
	{
		[YuzuMember]
		public float Spacing
		{
			get => spacing;
			set
			{
				if (spacing != value) {
					spacing = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private float spacing;

		public HBoxLayout()
		{
			DebugRectangles = new List<Rectangle>();
		}

		public override void ArrangeChildren()
		{
			ArrangementValid = true;
			var widgets = GetChildren();
			if (widgets.Count == 0) {
				return;
			}
			var constraints = new LinearAllocator.Constraints[widgets.Count];
			int i = 0;
			foreach (var child in widgets) {
				constraints[i++] = new LinearAllocator.Constraints {
					MinSize = child.EffectiveMinSize.X,
					MaxSize = child.EffectiveMaxSize.X,
					Stretch = EffectiveLayoutCell(child).StretchX
				};
			}
			var availableWidth = Math.Max(0, Owner.ContentWidth - (widgets.Count - 1) * Spacing);
			var sizes = LinearAllocator.Allocate(availableWidth, constraints, roundSizes: true);
			i = 0;
			DebugRectangles.Clear();
			var position = Owner.Padding.LeftTop;
			foreach (var child in widgets) {
				var size = new Vector2(sizes[i], Owner.ContentHeight);
				var align = EffectiveLayoutCell(child).Alignment;
				LayoutWidgetWithinCell(child, position, size, align, DebugRectangles);
				position.X += size.X + Spacing;
				i++;
			}
		}

		public override void MeasureSizeConstraints()
		{
			ConstraintsValid = true;
			var widgets = GetChildren();
			if (widgets.Count == 0) {
				Owner.MeasuredMinSize = Vector2.Zero;
				Owner.MeasuredMaxSize = Vector2.PositiveInfinity;
				return;
			}
			var minSize = new Vector2(
				widgets.Sum(i => i.EffectiveMinSize.X),
				widgets.Max(i => i.EffectiveMinSize.Y));
			var maxSize = new Vector2(
				widgets.Sum(i => i.EffectiveMaxSize.X),
				widgets.Max(i => i.EffectiveMaxSize.Y));
			var extraSpace = new Vector2((widgets.Count - 1) * Spacing, 0) + Owner.Padding;
			Owner.MeasuredMinSize = minSize + extraSpace;
			Owner.MeasuredMaxSize = maxSize + extraSpace;
		}

		public override NodeComponent Clone()
		{
			return (HBoxLayout)base.Clone();
		}
	}
}
