using System;
using System.Linq;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class VBoxLayout : Layout, ILayout
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

		public VBoxLayout()
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
			foreach (var w in widgets) {
				constraints[i++] = new LinearAllocator.Constraints {
					MinSize = w.EffectiveMinSize.Y,
					MaxSize = w.EffectiveMaxSize.Y,
					Stretch = EffectiveLayoutCell(w).StretchY
				};
			}
			var availableHeight = Math.Max(0, Owner.ContentHeight - (widgets.Count - 1) * Spacing);
			var sizes = LinearAllocator.Allocate(availableHeight, constraints, roundSizes: true);
			i = 0;
			DebugRectangles.Clear();
			var position = Owner.Padding.LeftTop;
			foreach (var child in widgets) {
				var size = new Vector2(Owner.ContentWidth, sizes[i]);
				var align = EffectiveLayoutCell(child).Alignment;
				LayoutWidgetWithinCell(child, position, size, align, DebugRectangles);
				position.Y += size.Y + Spacing;
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
				widgets.Max(i => i.EffectiveMinSize.X),
				widgets.Sum(i => i.EffectiveMinSize.Y));
			var maxSize = new Vector2(
				widgets.Max(i => i.EffectiveMaxSize.X),
				widgets.Sum(i => i.EffectiveMaxSize.Y));
			var extraSpace = new Vector2(0, (widgets.Count - 1) * Spacing) + Owner.Padding;
			Owner.MeasuredMinSize = minSize + extraSpace;
			Owner.MeasuredMaxSize = maxSize + extraSpace;
		}

		public override NodeComponent Clone()
		{
			return (VBoxLayout)base.Clone();
		}
	}
}
