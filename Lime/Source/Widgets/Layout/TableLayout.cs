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
			var cols = LinearAllocator.Allocate(widget.Width, CalcColConstraints(widget, cells), roundSizes: true);
			var rows = LinearAllocator.Allocate(widget.Height, CalcRowConstraints(widget, cells), roundSizes: true);
			// Layout each cell
			var p = Vector2.Zero;
			DebugRectangles.Clear();
			for (int i = 0; i < RowCount; i++) {
				p.X = 0;
				for (int j = 0; j < ColCount; j++) {
					var c = cells[i, j];
					if (c == null) {
						p.X += cols[j];
						continue;
					}
					// var offset = p + new Vector2(margin.Left, margin.Top);
					var colSpan = GetColSpan(c, j);
					var size = Vector2.Zero;
					// size.X = 0;// -margin.Left - GetCellMargin(widget, i, j + colSpan - 1).Right;
					for (int u = 0; u < colSpan; u++) {
						size.X += cols[j + u];
					}
					var rowSpan = GetRowSpan(c, i);
					// size.Y = 0;// -margin.Top - GetCellMargin(widget, i + rowSpan - 1, j).Bottom;
					for (int u = 0; u < rowSpan; u++) {
						size.Y += rows[i + u];
					}
					var margin = GetCellMargin(widget, i, j);
					margin.Right = GetCellMargin(widget, i, j + colSpan - 1).Right;
					margin.Bottom = GetCellMargin(widget, i + rowSpan - 1, j).Bottom;
					LayoutCell(c, p, size, margin, DebugRectangles);
					p.X += cols[j];
				}
				p.Y += rows[i];
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
						float margins = GetCellMargin(widget, i, j).Left + GetCellMargin(widget, i, j + s - 1).Right;
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
						float margins = GetCellMargin(widget, i, j).Top + GetCellMargin(widget, i + s - 1, j).Bottom;
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

		private Thickness GetCellMargin(Widget widget, int i, int j)
		{
			var padding = widget.Padding;
			return new Thickness() {
				Left = (j == 0) ? padding.Left : (ColSpacing / 2).Round(),
				Right = (j == ColCount - 1) ? padding.Right : (ColSpacing / 2).Round(),
				Top = (i == 0) ? padding.Top : (RowSpacing / 2).Round(),
				Bottom = (i == RowCount - 1) ? padding.Bottom : (RowSpacing / 2).Round()
			};
		}

		internal static void LayoutCell(Widget widget, Vector2 position, Vector2 size, Thickness margin, List<Rectangle> debugRectangles)
		{
			position += new Vector2(margin.Left, margin.Top);
			size -= margin;
			debugRectangles.Add(new Rectangle { A = position, B = position + size });
			var halign = GetCellData(widget).Alignment.X;
			var valign = GetCellData(widget).Alignment.Y;
			var innerSize = Vector2.Clamp(size, widget.MinSize, widget.MaxSize);
			if (halign == HAlignment.Right) {
				position.X += size.X - innerSize.X;
			} else if (halign == HAlignment.Center) {
				position.X += ((size.X - innerSize.X) / 2).Round();
			}
			if (valign == VAlignment.Bottom) {
				position.Y += size.Y - innerSize.Y;
			} else if (valign == VAlignment.Center) {
				position.Y += ((size.Y - innerSize.Y) / 2).Round();
			}
			widget.Position = position;
			widget.Size = innerSize;
			widget.Pivot = Vector2.Zero;
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
