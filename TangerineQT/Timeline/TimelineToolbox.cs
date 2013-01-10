using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public static class TimelineToolbox
	{
		static Document doc { get { return The.Document; } }

		public static int MaxColumn
		{
			get {
				int minCols = doc.LeftColumn + The.Timeline.KeyGrid.Width / doc.ColumnWidth;
				int numCols = Math.Max(RightmostKeyframeColumn + 1, minCols);
				return numCols;
			}
		}

		public static int NumberOfVisibleColumns
		{
			get {
				int numVisibleCols = The.Timeline.KeyGrid.Width / doc.ColumnWidth;
				return numVisibleCols;
			}
		}

		public static int NumberOfVisibleRows
		{
			get {
				int numVisibleRows = The.Timeline.KeyGrid.Height / doc.RowHeight;
				return numVisibleRows;
			}
		}

		public static int RightmostKeyframeColumn
		{
			get {
				int col = 0;
				foreach (var row in doc.Rows) {
					col = Math.Max(col, row.GetLastKeyframeColumn());
				}
				return col;
			}
		}
	}
}
