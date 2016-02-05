using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class TableLayout : CommonLayout, ILayout
	{
		public int RowCount { get; set; }
		public int ColCount { get; set; }
		public float ColSpacing;
		public float RowSpacing;
		public float Spacing { set { ColSpacing = RowSpacing = value; } }

		public TableLayout()
		{
			DebugRectangles = new List<Rectangle>();
		}

		public void OnSizeChanged(Widget widget, Vector2 sizeDelta)
		{
			ArrangeChildren(widget);
		}

		public override void MeasureSizeConstraints(Widget widget)
		{
			var cells = GetCellArray(widget.Nodes);
			if (cells == null) {
				return;
			}
			var cols = CalcColConstraints(widget, cells);
			var rows = CalcRowConstraints(widget, cells);
			var minSize = Vector2.Zero;
			var maxSize = Vector2.Zero;
			foreach (var i in cols) {
				minSize.X += i.MinSize;
				maxSize.X += i.MaxSize;
			}
			foreach (var i in rows) {
				minSize.Y += i.MinSize;
				maxSize.Y += i.MaxSize;
			}
			widget.MinSize = minSize;
			widget.MaxSize = maxSize;
		}

		public override void ArrangeChildren(Widget widget)
		{
			var cells = GetCellArray(widget.Nodes);
			if (cells == null)
				return;
			var availableWidth = widget.ContentWidth - (ColCount - 1) * ColSpacing;
			var availableHeight = widget.ContentHeight - (RowCount - 1) * RowSpacing;
			var cols = LinearAllocator.Allocate(availableWidth, CalcColConstraints(widget, cells), roundSizes: true);
			var rows = LinearAllocator.Allocate(availableHeight, CalcRowConstraints(widget, cells), roundSizes: true);
			// Layout each cell
			var p = new Vector2(0, widget.Padding.Top);
			DebugRectangles.Clear();
			for (int i = 0; i < RowCount; i++) {
				p.X = widget.Padding.Left;
				for (int j = 0; j < ColCount; j++) {
					var c = cells[i, j];
					if (c == null) {
						p.X += cols[j];
						continue;
					}
					var colSpan = GetColSpan(c, j);
					var size = Vector2.Zero;
					for (int u = 0; u < colSpan; u++) {
						size.X += cols[j + u] + (u > 0 ? ColSpacing : 0);
					}
					var rowSpan = GetRowSpan(c, i);
					for (int u = 0; u < rowSpan; u++) {
						size.Y += rows[i + u] + (u > 0 ? RowSpacing : 0);
					}
					LayoutWidgetWithinCell(c, p, size, DebugRectangles);
					p.X += cols[j] + ColSpacing;
				}
				p.Y += rows[i] + RowSpacing;
			}
		}

		private LinearAllocator.Constraints[] CalcColConstraints(Widget widget, Widget[,] cells)
		{
			var cols = new LinearAllocator.Constraints[ColCount];
			for (int i = 0; i < ColCount; i++) {
				cols[i] = new LinearAllocator.Constraints { MaxSize = float.PositiveInfinity };
			}
			for (int j = 0; j < ColCount; j++) {
				for (int i = 0; i < RowCount; i++) {
					var c = cells[i, j];
					if (c != null) {
						cols[j].Stretch = Math.Max(cols[j].Stretch, GetCellData(c).StretchX);
						int s = GetColSpan(c, j);
						float mn = c.MinSize.X;
						float mx = c.MaxSize.X;
						if (s > 1) {
							mn /= s;
							mx /= s;
						}
						// Distribute constraints evenly
						for (int u = j; u < s + j; u++) {
							cols[u].MinSize = Math.Max(cols[u].MinSize, mn);
							cols[u].MaxSize = Math.Max(cols[u].MaxSize, mx);
						}
					}
				}
			}
			return cols;
		}

		private LinearAllocator.Constraints[] CalcRowConstraints(Widget widget, Widget[,] cells)
		{
			var rows = new LinearAllocator.Constraints[RowCount];
			for (int i = 0; i < RowCount; i++) {
				rows[i] = new LinearAllocator.Constraints { MaxSize = float.PositiveInfinity };
			}
			for (int i = 0; i < RowCount; i++) {
				for (int j = 0; j < ColCount; j++) {
					var c = cells[i, j];
					if (c != null) {
						rows[i].Stretch = Math.Max(rows[i].Stretch, GetCellData(c).StretchY);
						int s = GetRowSpan(c, i);
						float mn = c.MinSize.Y;
						float mx = c.MaxSize.Y;
						if (s > 1) {
							mn /= s;
							mx /= s;
						}
						// Distribute constraints evenly
						for (int u = i; u < s + i; u++) {
							rows[u].MinSize = Math.Max(rows[u].MinSize, mn);
							rows[u].MaxSize = Math.Max(rows[u].MaxSize, mx);
						}
					}
				}
			}
			return rows;
		}

		private Widget[,] GetCellArray(NodeList nodes)
		{
			var cells = new Widget[RowCount, ColCount];
			var occupied = new bool[RowCount, ColCount];
			int t = 0;
			for (int i = 0; i < RowCount; i++) {
				for (int j = 0; j < ColCount; j++) {
					if (occupied[i, j])
						continue;
					Widget c = null;
					while (c == null) {
						if (t >= nodes.Count)
							return cells;
						c = nodes[t++].AsWidget;
					}
					cells[i, j] = c;
					int rowSpan = GetRowSpan(c, i);
					int colSpan = GetColSpan(c, j);
					for (int u = 0; u < rowSpan; u++) {
						for (int v = 0; v < colSpan; v++) {
							occupied[u + i, v + j] = true;
						}
					}
				}
			}
			return cells;
		}

		private int GetRowSpan(Widget cell, int row)
		{
			var cd = GetCellData(cell);
			return Mathf.Clamp(cd.RowSpan, 1, RowCount - row);
		}

		private int GetColSpan(Widget cell, int column)
		{
			var cd = GetCellData(cell);
			return Mathf.Clamp(cd.ColSpan, 1, ColCount - column);
		}
	}
}
