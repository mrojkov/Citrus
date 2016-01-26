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

		public void OnSizeChanged(Widget widget, Vector2 sizeDelta)
		{
			// Size changing could only affect children arrangement, not the widget's size constraints.
			InvalidateArrangement(widget);
		}

		public override void ArrangeChildren(Widget widget)
		{
			ArrangementValid = true;
			var widgets = widget.Nodes.OfType<Widget>().ToList();
			if (widgets.Count == 0) {
				return;
			}
			var constraints = new LinearAllocator.Constraints[widgets.Count];
			var margins = CalcCellMargins(widget.Padding, widgets.Count);
			int i = 0;
			foreach (var w in widgets) {
				var extraSpace = margins[i].Left + margins[i].Right;
				constraints[i++] = new LinearAllocator.Constraints {
					MinSize = w.MinSize.X + extraSpace,
					MaxSize = w.MaxSize.X + extraSpace,
					Stretch = (w.LayoutCell ?? LayoutCell.Default).StretchX
				};
			}
			var sizes = LinearAllocator.Allocate(widget.Width, constraints, roundSizes: true);
			i = 0;
			DebugRectangles.Clear();
			var position = Vector2.Zero;
			foreach (var w in widgets) {
				var size = new Vector2(sizes[i], Mathf.Clamp(widget.Height, w.MinHeight, w.MaxHeight));
				TableLayout.LayoutCell(w, position, size, margins[i], DebugRectangles);
				position.X += size.X;
				i++;
			}
		}

		private Thickness[] CalcCellMargins(Thickness padding, int numCells)
		{
			var margins = new Thickness[numCells];
			for (int i = 0; i < numCells; i++) {
				margins[i] = new Thickness {
					Left = (i == 0) ? padding.Left : (Spacing / 2).Round(),
					Right = (i == numCells - 1) ? padding.Right : (Spacing / 2).Round(),
					Top = padding.Top,
					Bottom = padding.Bottom,
				};
			}
			return margins;
		}

		public override void MeasureSizeConstraints(Widget widget)
		{
			ConstraintsValid = true;
			var widgets = widget.Nodes.OfType<Widget>().ToList();
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