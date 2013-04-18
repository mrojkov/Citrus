using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	[Flags]
	public enum CanvasLayoutFlags
	{
		None = 0,
		Expand = 1
	}

	public class CanvasPlacement
	{
		public Xwt.Widget Widget;
		public CanvasLayoutFlags Flags;
		public double Space;

		public double GetWidth()
		{
			if (Widget == null) {
				return Space;
			}
			if ((Flags & CanvasLayoutFlags.Expand) != 0) {
				return 0;
			} else {
				return Widget.Surface.GetPreferredWidth().NaturalSize;
			}
		}

		public double GetHeight()
		{
			double h = Widget.Surface.GetPreferredHeight().NaturalSize;
			if (Widget is Xwt.TextEntry && h == 0) {
				// Баг: для WPF.TextEntry высота по-умолчанию 0
				h = 25;
			}
			return h;
		}
	}

	public class CanvasRowLayout
	{
		List<CanvasPlacement> items = new List<CanvasPlacement>();
		Xwt.Canvas canvas;
		double top;
		double height;

		public CanvasRowLayout(Xwt.Canvas canvas, double top, double height)
		{
			this.canvas = canvas;
			this.top = top;
			this.height = height;
		}

		public void AddSpace(double width)
		{
			items.Add(new CanvasPlacement { Space = width });
		}

		public void Add(Xwt.Widget widget, CanvasLayoutFlags flags = 0)
		{
			items.Add(new CanvasPlacement { Widget = widget, Flags = flags });
		}

		public void Realize()
		{
			double w = 0;
			foreach (var i in items) {
				w += i.GetWidth();
			}
			double x = 0;
			foreach (var i in items) {
				double width = i.GetWidth();
				if ((i.Flags & CanvasLayoutFlags.Expand) != 0) {
					width = canvas.Bounds.Width - w;
				}
				if (i.Widget != null) {
					SetItemBounds(i, x, width);
				}
				x += width;
			}
		}

		void SetItemBounds(CanvasPlacement item, double left, double width)
		{
			var b = canvas.GetChildBounds(item.Widget);
			b.Left = left;
			b.Width = width;
			b.Top = top + (height - item.GetHeight()) / 2;
			b.Height = item.GetHeight();
			canvas.SetChildBounds(item.Widget, b);
		}
	}
}
