using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine.Timeline
{
	public static class Toolbox
	{
		static Document doc { get { return The.Document; } }

		public static int CalcLastColumn()
		{
			int minCols = doc.LeftColumn + (int)(The.Timeline.Grid.Width / doc.ColumnWidth);
			int lastColumn = Math.Max(CalcRightmostKeyframeColumn() + 1, minCols);
			lastColumn = Math.Max(The.Document.CurrentColumn, lastColumn);
			return lastColumn;
		}

		public static int CalcNumberOfVisibleColumns()
		{
			int numVisibleCols = (int)(The.Timeline.Grid.Width / doc.ColumnWidth);
			return numVisibleCols;
		}

		public static int CalcNumberOfVisibleRows()
		{
			int numVisibleRows = (int)(The.Timeline.Grid.Height / doc.RowHeight);
			return numVisibleRows;
		}

		public static int CalcRightmostKeyframeColumn()
		{
			int col = 0;
			foreach (var row in doc.Rows) {
				col = Math.Max(col, row.GetLastKeyframeColumn());
			}
			return col;
		}

		public static int PixelToColumn(double x)
		{
			int column = (int)(x / doc.ColumnWidth) + doc.LeftColumn;
			return column;
		}

		public static int PixelToRow(double y)
		{
			int row = (int)(y / doc.RowHeight) + doc.TopRow;
			return row;
		}

		public static Lime.IntVector2 PixelToCell(Xwt.Point p)
		{
			return new Lime.IntVector2(PixelToColumn(p.X), PixelToRow(p.Y));
		}

		//public static bool IsLeftButtonPressed()
		//{
		//	return (QApplication.MouseButtons() & Qt.MouseButton.LeftButton) != 0;
		//}
	}
}
