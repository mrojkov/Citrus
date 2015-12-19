using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class TableLayout : CommonLayout, ILayout
	{
		public int RowCount { get; set; }
		public int ColCount { get; set; }
		public Margin Margin = new Margin();
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
			List<LinearAllocator.Constraints> cols, rows;
			CalcCellConstraints(cells, out cols, out rows);
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
			List<LinearAllocator.Constraints> cols, rows;
			CalcCellConstraints(cells, out cols, out rows);
			var allocator = new LinearAllocator(roundSizes: true);
			allocator.Allocate(widget.Width, cols);
			allocator.Allocate(widget.Height, rows);
			// Layout each cell
			var p = Vector2.Zero;
			DebugRectangles.Clear();
			for (int i = 0; i < RowCount; i++) {
				p.X = 0;
				for (int j = 0; j < ColCount; j++) {
					var c = cells[i, j];
					if (c == null) {
						p.X += cols[j].Size;
						continue;
					}
					var margin = GetCellMargin(i, j);
					var offset = p + new Vector2(margin.Left, margin.Top);
					var colSpan = GetColSpan(c, j);
					Vector2 size;
					size.X = -margin.Left - GetCellMargin(i, j + colSpan - 1).Right;
					for (int u = 0; u < colSpan; u++) {
						size.X += cols[j + u].Size;
					}
					var rowSpan = GetRowSpan(c, i);
					size.Y = -margin.Top - GetCellMargin(i + rowSpan - 1, j).Bottom;
					for (int u = 0; u < rowSpan; u++) {
						size.Y += rows[i + u].Size;
					}
					var halign = GetCellData(c).Alignment.X;
					var valign = GetCellData(c).Alignment.Y;
					LayoutCell(c, halign, valign, offset, size);
					p.X += cols[j].Size;
				}
				p.Y += rows[i].Size;
			}
		}

		private void CalcCellConstraints(Widget[,] cells, out List<LinearAllocator.Constraints> cols, out List<LinearAllocator.Constraints> rows)
		{
			cols = new List<LinearAllocator.Constraints>(ColCount);
			for (int i = 0; i < ColCount; i++) {
				cols.Add(new LinearAllocator.Constraints { MaxSize = float.PositiveInfinity });
			}
			for (int j = 0; j < ColCount; j++) {
				for (int i = 0; i < RowCount; i++) {
					var c = cells[i, j];
					if (c != null) {
						cols[j].Stretch = Math.Max(cols[j].Stretch, GetCellData(c).StretchX);
						int s = GetColSpan(c, j);
						float margins = GetCellMargin(i, j).Left + GetCellMargin(i, j + s - 1).Right;
						float mn = c.MinSize.X + margins;
						float mx = c.MaxSize.X + margins;
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

			rows = new List<LinearAllocator.Constraints>(RowCount);
			for (int i = 0; i < RowCount; i++) {
				rows.Add(new LinearAllocator.Constraints { MaxSize = float.PositiveInfinity });
			}
			for (int i = 0; i < RowCount; i++) {
				for (int j = 0; j < ColCount; j++) {
					var c = cells[i, j];
					if (c != null) {
						rows[i].Stretch = Math.Max(rows[i].Stretch, GetCellData(c).StretchY);
						int s = GetRowSpan(c, i);
						float margins = GetCellMargin(i, j).Top + GetCellMargin(i + s - 1, j).Bottom;
						float mn = c.MinSize.Y + margins;
						float mx = c.MaxSize.Y + margins;
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

		private Margin GetCellMargin(int i, int j)
		{
			return new Margin() {
				Left = (j == 0) ? Margin.Left : (ColSpacing / 2).Round(),
				Right = (j == ColCount - 1) ? Margin.Right : (ColSpacing / 2).Round(),
				Top = (i == 0) ? Margin.Top : (RowSpacing / 2).Round(),
				Bottom = (i == RowCount - 1) ? Margin.Bottom : (RowSpacing / 2).Round()
			};
		}

		private void LayoutCell(Widget widget, HAlignment halign, VAlignment valign, Vector2 position, Vector2 cellSize)
		{
			DebugRectangles.Add(new Rectangle { A = position, B = position + cellSize });
			var size = Vector2.Clamp(cellSize, widget.MinSize, widget.MaxSize);
			if (halign == HAlignment.Right) {
				position.X += cellSize.X - size.X;
			} else if (halign == HAlignment.Center) {
				position.X += ((cellSize.X - size.X) / 2).Round();
			}
			if (valign == VAlignment.Bottom) {
				position.Y += cellSize.Y - size.Y;
			} else if (valign == VAlignment.Center) {
				position.Y += ((cellSize.Y - size.Y) / 2).Round();
			}
			widget.Position = position;
			widget.Size = size;
		}

		private static LayoutCell GetCellData(Widget cell)
		{
			return cell.LayoutCell ?? Lime.LayoutCell.Default;
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
