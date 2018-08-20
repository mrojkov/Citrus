using System;
using System.Linq;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class TableLayout : Layout, ILayout
	{
		[YuzuMember]
		public int RowCount
		{
			get => rowCount;
			set
			{
				if (rowCount != value) {
					rowCount = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private int rowCount;

		[YuzuMember]
		public int ColumnCount
		{
			get => columnCount;
			set
			{
				if (columnCount != value) {
					columnCount = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private int columnCount;

		[YuzuMember]
		public float ColumnSpacing
		{
			get => columnSpacing;
			set
			{
				if (columnSpacing != value) {
					columnSpacing = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private float columnSpacing;


		[YuzuMember]
		public float RowSpacing
		{
			get => rowSpacing;
			set {
				if (rowSpacing != value) {
					rowSpacing = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private float rowSpacing;

		public float Spacing { set { ColumnSpacing = RowSpacing = value; } }

		[YuzuMember]
		public List<LayoutCell> ColumnDefaults = new List<LayoutCell>();

		[YuzuMember]
		public List<LayoutCell> RowDefaults = new List<LayoutCell>();

		public TableLayout()
		{
			DebugRectangles = new List<Rectangle>();
		}

		public override void MeasureSizeConstraints()
		{
			ConstraintsValid = true;
			var cells = GetCellArray(Owner.Nodes);
			if (cells == null) {
				Owner.MeasuredMinSize = Vector2.Zero;
				Owner.MeasuredMaxSize = Vector2.PositiveInfinity;
				return;
			}
			var columns = CalcColConstraints(Owner, cells);
			var rows = CalcRowConstraints(Owner, cells);
			var minSize = Vector2.Zero;
			var maxSize = Vector2.Zero;
			foreach (var i in columns) {
				minSize.X += i.MinSize;
				maxSize.X += i.MaxSize;
			}
			foreach (var i in rows) {
				minSize.Y += i.MinSize;
				maxSize.Y += i.MaxSize;
			}
			// TODO: totalSpacing should take in account col/row spans.
			var totalSpacing = new Vector2(ColumnSpacing * (ColumnCount - 1), RowSpacing * (RowCount - 1));
			Owner.MeasuredMinSize = minSize + totalSpacing;
			Owner.MeasuredMaxSize = maxSize + totalSpacing;
		}

		public override void ArrangeChildren()
		{
			ArrangementValid = true;
			var cells = GetCellArray(Owner.Nodes);
			if (cells == null) {
				return;
			}
			var availableWidth = Owner.ContentWidth - (ColumnCount - 1) * ColumnSpacing;
			var availableHeight = Owner.ContentHeight - (RowCount - 1) * RowSpacing;
			var cols = LinearAllocator.Allocate(availableWidth, CalcColConstraints(Owner, cells), roundSizes: true);
			var rows = LinearAllocator.Allocate(availableHeight, CalcRowConstraints(Owner, cells), roundSizes: true);
			// Layout each cell
			var p = new Vector2(0, Owner.Padding.Top);
			DebugRectangles.Clear();
			for (int i = 0; i < RowCount; i++) {
				p.X = Owner.Padding.Left;
				for (int j = 0; j < ColumnCount; j++) {
					var c = cells[i, j];
					if (c == null) {
						p.X += cols[j];
						continue;
					}
					var columnSpan = GetColumnSpan(c, i, j);
					var size = Vector2.Zero;
					for (int u = 0; u < columnSpan; u++) {
						size.X += cols[j + u] + (u > 0 ? ColumnSpacing : 0);
					}
					var rowSpan = GetRowSpan(c, i, j);
					for (int u = 0; u < rowSpan; u++) {
						size.Y += rows[i + u] + (u > 0 ? RowSpacing : 0);
					}
					var align = EffectiveLayoutCell(c, i, j).Alignment;
					LayoutWidgetWithinCell(c, p, size, align, DebugRectangles);
					p.X += cols[j] + ColumnSpacing;
				}
				p.Y += rows[i] + RowSpacing;
			}
		}

		private LayoutCell EffectiveLayoutCell(Widget cell, int row, int column)
		{
			return cell.LayoutCell
			       ?? (ColumnDefaults.Count > column
				       ? ColumnDefaults[column]
				       : (RowDefaults.Count > row
					       ? RowDefaults[row]
					       : (DefaultCell ?? LayoutCell.Default)));
		}

		private LinearAllocator.Constraints[] CalcColConstraints(Widget widget, Widget[,] cells)
		{
			var cols = new LinearAllocator.Constraints[ColumnCount];
			for (int i = 0; i < ColumnCount; i++) {
				cols[i] = new LinearAllocator.Constraints { MaxSize = float.PositiveInfinity };
			}
			for (int j = 0; j < ColumnCount; j++) {
				for (int i = 0; i < RowCount; i++) {
					var c = cells[i, j];
					if (c != null) {
						cols[j].Stretch = Math.Max(cols[j].Stretch, EffectiveLayoutCell(c, i, j).StretchX);
						int s = GetColumnSpan(c, i, j);
						float mn = c.EffectiveMinSize.X;
						float mx = c.EffectiveMaxSize.X;
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
				for (int j = 0; j < ColumnCount; j++) {
					var c = cells[i, j];
					if (c != null) {
						rows[i].Stretch = Math.Max(rows[i].Stretch, EffectiveLayoutCell(c, i, j).StretchY);
						int s = GetRowSpan(c, i, j);
						float mn = c.EffectiveMinSize.Y;
						float mx = c.EffectiveMaxSize.Y;
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
			Widget[,] cells = null;
			var occupied = new bool[RowCount, ColumnCount];
			int t = 0;
			for (int i = 0; i < RowCount; i++) {
				for (int j = 0; j < ColumnCount; j++) {
					if (occupied[i, j])
						continue;
					Widget c = null;
					while (c == null) {
						if (t >= nodes.Count)
							return cells;
						c = nodes[t++].AsWidget;
					}
					if (cells == null && c != null) {
						cells = new Widget[RowCount, ColumnCount];
					}
					cells[i, j] = c;
					int rowSpan = GetRowSpan(c, i, j);
					int columnSpan = GetColumnSpan(c, i, j);
					for (int u = 0; u < rowSpan; u++) {
						for (int v = 0; v < columnSpan; v++) {
							occupied[u + i, v + j] = true;
						}
					}
				}
			}
			return cells;
		}

		private int GetRowSpan(Widget cell, int row, int column)
		{
			var cd = EffectiveLayoutCell(cell, row, column);
			return Mathf.Clamp(cd.RowSpan, 1, RowCount - row);
		}

		private int GetColumnSpan(Widget cell, int row, int column)
		{
			var cd = EffectiveLayoutCell(cell, row, column);
			return Mathf.Clamp(cd.ColumnSpan, 1, ColumnCount - column);
		}

		public override NodeComponent Clone()
		{
			var clone = (TableLayout)base.Clone();
			clone.ColumnDefaults = new List<LayoutCell>();
			clone.RowDefaults = new List<LayoutCell>();
			return clone;
		}
	}
}
