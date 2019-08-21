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
		public bool HorizontallySizeable
		{
			get => horizontallySizeable;
			set
			{
				if (horizontallySizeable != value) {
					horizontallySizeable = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private bool horizontallySizeable;

		[YuzuMember]
		public bool VerticallySizeable
		{
			get => verticallySizeable;
			set {
				if (verticallySizeable != value) {
					verticallySizeable = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private bool verticallySizeable;

		public StackLayout()
		{
			DebugRectangles = new List<Rectangle>();
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
			var minSize = new Vector2(widgets.Max(i => i.EffectiveMinSize.X), widgets.Max(i => i.EffectiveMinSize.Y)) + Owner.Padding;
			var maxSize = new Vector2(widgets.Max(i => i.EffectiveMaxSize.X), widgets.Max(i => i.EffectiveMaxSize.Y)) + Owner.Padding;
			if (HorizontallySizeable) {
				minSize.X = 0;
				maxSize.X = float.PositiveInfinity;
			}
			if (VerticallySizeable) {
				minSize.Y = 0;
				maxSize.Y = float.PositiveInfinity;
			}
			Owner.MeasuredMinSize = minSize;
			Owner.MeasuredMaxSize = maxSize;
		}

		public override void ArrangeChildren()
		{
			DebugRectangles.Clear();
			ArrangementValid = true;
			foreach (var child in GetChildren()) {
				var position = Owner.ContentPosition;
				var size = Owner.ContentSize;
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
	}
}
