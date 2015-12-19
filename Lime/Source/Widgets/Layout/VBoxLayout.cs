using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class VBoxLayout : CommonLayout, ILayout
	{
		public VBoxLayout()
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
			var items = new List<LinearAllocator.Constraints>(widgets.Count);
			foreach (var w in widgets) {
				var cell = w.LayoutCell ?? LayoutCell.Default;
				items.Add(new LinearAllocator.Constraints {
					MinSize = w.MinSize.Y,
					MaxSize = w.MaxSize.Y,
					Stretch = cell.StretchY
				});
			}
			var packer = new LinearAllocator(roundSizes: true);
			packer.Allocate(widget.Height, items);
			float y = 0;
			int i = 0;
			DebugRectangles.Clear();
			foreach (var w in widgets) {
				w.Size = new Vector2(widget.Width, items[i++].Size);
				w.Position = new Vector2(0, y);
				w.Pivot = Vector2.Zero;
				y += w.Height;
				DebugRectangles.Add(new Rectangle { A = w.Position, B = w.Position + w.Size });
			}
		}

		public override void MeasureSizeConstraints(Widget widget)
		{
			ConstraintsValid = true;
			var widgets = widget.Nodes.OfType<Widget>().ToList();
			var minSize = new Vector2(
				widgets.Max(i => i.MinSize.X),
				widgets.Sum(i => i.MinSize.Y)
			);
			var maxSize = new Vector2(
				widgets.Max(i => i.MaxSize.X),
				widgets.Sum(i => i.MaxSize.Y)
			);
			widget.MinSize = minSize;
			widget.MaxSize = maxSize;
		}
	}
}