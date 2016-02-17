using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class FlowLayout : CommonLayout, ILayout
	{
		private List<int> splitIndices = new List<int>();
		public float Spacing { get; set; }

		public FlowLayout()
		{
			DebugRectangles = new List<Rectangle>();
		}

		public void OnSizeChanged(Widget widget, Vector2 sizeDelta)
		{
			InvalidateConstraintsAndArrangement(widget);
		}

		public override void ArrangeChildren(Widget widget)
		{
			ArrangementValid = true;
			var widgets = GetChildren(widget);
			if (widgets.Count == 0) {
				return;
			}
			DebugRectangles.Clear();
			float dy = 0.0f;
			for (int j = 0; j < splitIndices.Count - 1; j++) {
				int i0 = splitIndices[j];
				int i1 = splitIndices[j + 1];
				var constraints = new LinearAllocator.Constraints[i1 - i0];
				var line = widgets.GetRange(i0, i1 - i0);
				var maxLineHeight = line.Max((w) => w.Height);
				var availableLength = Math.Max(0, widget.ContentWidth - (line.Count - 1) * Spacing);
				int i = 0;
				foreach (var w in line) {
					constraints[i++] = new LinearAllocator.Constraints {
						MinSize = w.MinSize.X,
						MaxSize = w.MaxSize.X,
						Stretch = (w.LayoutCell ?? LayoutCell.Default).StretchX
					};
				}
				var sizes = LinearAllocator.Allocate(availableLength, constraints, roundSizes: true);
				i = 0;
				var position = new Vector2(widget.Padding.Left, widget.Padding.Top + dy);
				foreach (var w in line) {
					var height = (w.LayoutCell ?? LayoutCell.Default).Stretch.Y == 0.0f
						? w.MinHeight
						: maxLineHeight;
					var size = new Vector2(sizes[i], height);
					LayoutWidgetWithinCell(w, position, size, DebugRectangles);
					position.X += size.X + Spacing;
					i++;
				}
				dy += maxLineHeight + Spacing;
			}
		}

		public override void MeasureSizeConstraints(Widget widget)
		{
			ConstraintsValid = true;
			var widgets = GetChildren(widget);
			float dx = widget.Padding.Left + widget.Padding.Right;
			float dy = widget.Padding.Top + widget.Padding.Bottom - Spacing;
			float maxdx = dx;
			float maxrowdy = 0;
			int i = 0;
			splitIndices.Clear();
			splitIndices.Add(i);
			while (i < widgets.Count) {
				var w = widgets[i];
				dx += w.MinWidth;
				if (dx > widget.Width || i + 1 == widgets.Count) {
					if (dx > widget.Width) {
						splitIndices.Add(i);
						i--;
						dx -= w.MinWidth + Spacing;
					} else {
						splitIndices.Add(i + 1);
						maxrowdy = Mathf.Max(maxrowdy, w.MinHeight);
					}
					maxdx = Mathf.Max(maxdx, dx);
					dx = widget.Padding.Left + widget.Padding.Right;
					dy += maxrowdy + Spacing;
					maxrowdy = 0.0f;
				} else {
					maxrowdy = Mathf.Max(maxrowdy, w.MinHeight);
					dx += Spacing;
				}
				i++;
			}
			widget.Height = widget.MinHeight = widget.MaxHeight = dy;
		}
	}
}