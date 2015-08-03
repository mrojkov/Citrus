using System;
using System.Linq;

namespace Lime
{
	public class TableLayoutCell
	{
		public HAlignment HAlignment = HAlignment.Center;
		public VAlignment VAlignment = VAlignment.Center;
		public int ColumnSpan = 1;
		public int RowSpan = 1;

		public static readonly TableLayoutCell Default = new TableLayoutCell();
	}

	public class TableLayout : Layout
	{
		public int RowCount { get; set; }
		public int ColumnCount { get; set; }
		public int RowToStretch { get; set; }
		public int ColumnToStretch { get; set; }
		public Margin Margin = new Margin();
		public float ColumnSpacing;
		public float RowSpacing;
		public float Spacing
		{
			get { return ColumnSpacing == RowSpacing ? ColumnSpacing : float.NaN; }
			set { ColumnSpacing = RowSpacing = value; }
		}

		private Widget widget;

		public override void OnSizeChanged(Widget widget, Vector2 sizeDelta)
		{
			Refresh(widget);
		}

		private void Refresh(Widget widget)
		{
			this.widget = widget;
			Refresh();
			this.widget = null;
		}

		private void Refresh()
		{
			int maxRowSpan, maxColSpan;
			var cells = EnumerateCells(out maxRowSpan, out maxColSpan);
			if (cells == null)
				return;
			// Calculate the minimum width for each column
			var colWidths = new float[ColumnCount];
			for (int s = 1; s <= maxColSpan; s++) {
				for (int i = 0; i < RowCount; i++) {
					for (int j = 0; j < ColumnCount; j++) {
						var c = cells[i, j];
						if (c == null || GetColSpan(c, j) != s || GetCellData(c).HAlignment == HAlignment.Expand)
                            continue;
						var requiredWidth = c.Width + GetCellMargin(i, j).Left + GetCellMargin(i, j + s - 1).Right;
						var currentWidth = 0f;
						for (int u = 0; u < s; u++) {
							currentWidth += colWidths[u + j];
						}
						if (requiredWidth > currentWidth) {
							var m = requiredWidth / s;
							for (int u = 0; u < s; u++) {
								colWidths[u + j] = Math.Max(m, colWidths[u + j]);
							}
						}
					}
				}
			}
			// Calculate the minimum height for each row
			var rowHeights = new float[RowCount];
			for (int s = 1; s <= maxRowSpan; s++) {
				for (int i = 0; i < RowCount; i++) {
					for (int j = 0; j < ColumnCount; j++) {
						var c = cells[i, j];
						if (c == null || GetRowSpan(c, i) != s || GetCellData(c).VAlignment == VAlignment.Expand)
							continue;
						var requiredHeight = c.Height + GetCellMargin(i, j).Top + GetCellMargin(i + s - 1, j).Bottom;
						var currentHeight = 0f;
						for (int u = 0; u < s; u++) {
							currentHeight += rowHeights[u + i];
						}
						if (requiredHeight > currentHeight) {
							var m = requiredHeight / s;
							for (int u = 0; u < s; u++) {
								rowHeights[u + i] = Math.Max(m, rowHeights[u + i]);
							}
						}
					}
				}
			}
			// Stretch given row/column
            colWidths[ColumnToStretch] = Math.Max(widget.Width - colWidths.Sum(), colWidths[ColumnToStretch]);
			rowHeights[RowToStretch] = Math.Max(widget.Height - rowHeights.Sum(), rowHeights[RowToStretch]);
			// Layout each cell
			var p = Vector2.Zero;
			for (int i = 0; i < RowCount; i++) {
				p.X = 0;
				for (int j = 0; j < ColumnCount; j++) {
					var c = cells[i, j];
					if (c == null) {
						p.X += colWidths[j];
						continue;
					}
                    var margin = GetCellMargin(i, j);
					var offset = p + new Vector2(margin.Left, margin.Top);
					var colSpan = GetColSpan(c, j);
					Vector2 size;
					size.X = -margin.Left - GetCellMargin(i, j + colSpan - 1).Right;
					for (int u = 0; u < colSpan; u++) {
						size.X += colWidths[j + u];
					}
					var rowSpan = GetRowSpan(c, i);
					size.Y = -margin.Top - GetCellMargin(i + rowSpan - 1, j).Bottom;
					for (int u = 0; u < rowSpan; u++) {
						size.Y += rowHeights[i + u];
					}
					var halign = GetCellData(c).HAlignment;
					var valign = GetCellData(c).VAlignment;
					LayoutCell(c, halign, valign, offset, size);
					p.X += colWidths[j];
				}
				p.Y += rowHeights[i];
			}
		}

		private Widget[,] EnumerateCells(out int maxRowSpan, out int maxColSpan)
		{
			var cells = new Widget[RowCount, ColumnCount];
			var occupied = new bool[RowCount, ColumnCount];
			int t = 0;
			maxColSpan = maxRowSpan = 0;
            for (int i = 0; i < RowCount; i++) {
				for (int j = 0; j < ColumnCount; j++) {
					if (occupied[i, j])
						continue;
					if (t >= widget.Nodes.Count)
						return null;
					var c = widget.Nodes[t].AsWidget;
					cells[i, j] = c;
					int rowSpan = GetRowSpan(c, i);
					int colSpan = GetColSpan(c, j);
					for (int u = 0; u < rowSpan; u++) {
						for (int v = 0; v < colSpan; v++) {
							occupied[u + i, v + j] = true;
						}
					}
					maxRowSpan = Math.Max(maxRowSpan, rowSpan);
					maxColSpan = Math.Max(maxColSpan, colSpan);
					t++;
                }
			}
			return cells;
		}

		private Margin GetCellMargin(int i, int j)
		{
			return new Margin() {
				Left = (j == 0) ? Margin.Left : ColumnSpacing / 2,
				Right = (j == ColumnCount - 1) ? Margin.Right : ColumnSpacing / 2,
				Top = (i == 0) ? Margin.Top : RowSpacing / 2,
				Bottom = (i == RowCount - 1) ? Margin.Bottom : RowSpacing / 2
			};
		}

		private void LayoutCell(Widget widget, HAlignment halign, VAlignment valign, Vector2 position, Vector2 size)
		{
			if (halign == HAlignment.Left) {
				size.X = widget.Width;
			} else if (halign == HAlignment.Right) {
				position.X += size.X - widget.Width;
				size.X = widget.Width;
			} else if (halign == HAlignment.Center) {
				position.X += (size.X - widget.Width) / 2;
				size.X = widget.Width;
			}
			if (valign == VAlignment.Top) {
				size.Y = widget.Height;
			} else if (valign == VAlignment.Bottom) {
				position.Y += size.Y - widget.Height;
				size.Y = widget.Height;
			} else if (valign == VAlignment.Center) {
				position.Y += (size.Y - widget.Height) / 2;
				size.Y = widget.Height;
			}
			widget.Position = position;
			widget.Size = size;
		}

		private static TableLayoutCell GetCellData(Widget cell)
		{
			return (cell.LayoutCell as TableLayoutCell) ?? TableLayoutCell.Default;
		}

		private int GetRowSpan(Widget cell, int row)
		{
			var cd = GetCellData(cell);
			return Mathf.Clamp(cd.RowSpan, 1, RowCount - row);
		}

		private int GetColSpan(Widget cell, int column)
		{
			var cd = GetCellData(cell);
			return Mathf.Clamp(cd.ColumnSpan, 1, ColumnCount - column);
		}
	}
}
