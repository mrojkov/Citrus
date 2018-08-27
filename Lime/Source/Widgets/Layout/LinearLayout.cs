using System;
using System.Collections.Generic;
using System.Linq;
using Yuzu;

namespace Lime
{
	public class HBoxLayout : LinearLayout
	{
		public HBoxLayout() : base(LayoutDirection.LeftToRight)
		{ }
	}

	public class VBoxLayout : LinearLayout
	{
		public VBoxLayout() : base(LayoutDirection.TopToBottom)
		{ }
	}

	[TangerineRegisterComponent]
	public class LinearLayout : Layout, ILayout
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

		[YuzuMember]
		public LayoutDirection Direction
		{
			get {
				return direction;
			}
			set {
				if (direction != value) {
					direction = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private LayoutDirection direction = LayoutDirection.LeftToRight;

		public LinearLayout()
		{
			DebugRectangles = new List<Rectangle>();
		}

		public LinearLayout(LayoutDirection direction) : this()
		{
			Direction = direction;
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

			if (direction == LayoutDirection.LeftToRight) {
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
			} else if (direction == LayoutDirection.TopToBottom) {
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
			Vector2 extraSpace = Vector2.Zero;
			Vector2 minSize = Vector2.Zero;
			Vector2 maxSize = Vector2.Zero;
			if (direction == LayoutDirection.LeftToRight) {
				minSize = new Vector2(
					widgets.Sum(i => i.EffectiveMinSize.X),
					widgets.Max(i => i.EffectiveMinSize.Y));
				maxSize = new Vector2(
					widgets.Sum(i => i.EffectiveMaxSize.X),
					widgets.Max(i => i.EffectiveMaxSize.Y));
				extraSpace = new Vector2((widgets.Count - 1) * Spacing, 0) + Owner.Padding;
			} else if (direction == LayoutDirection.TopToBottom) {
				minSize = new Vector2(
					widgets.Max(i => i.EffectiveMinSize.X),
					widgets.Sum(i => i.EffectiveMinSize.Y));
				maxSize = new Vector2(
					widgets.Max(i => i.EffectiveMaxSize.X),
					widgets.Sum(i => i.EffectiveMaxSize.Y));
				extraSpace = new Vector2(0, (widgets.Count - 1) * Spacing) + Owner.Padding;
			}
			Owner.MeasuredMinSize = minSize + extraSpace;
			Owner.MeasuredMaxSize = maxSize + extraSpace;
		}

		public override NodeComponent Clone()
		{
			var clone = (LinearLayout)base.Clone();
			clone.DebugRectangles = new List<Rectangle>();
			return clone;
		}
	}
}
