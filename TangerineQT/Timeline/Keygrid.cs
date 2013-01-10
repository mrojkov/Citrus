using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;
using System.Runtime.InteropServices;

namespace Tangerine
{
	public class KeyGrid : QWidget
	{
		Document doc { get { return The.Document; } }

		public static int MaxLinesPerRow = 5;

		public KeyGrid()
		{
			Paint += Keygrid_Paint;
		}

		void Keygrid_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			int numRows = Size.Height / doc.RowHeight + 1;
			int numCols = TimelineToolbox.MaxColumn; // Size.Width / doc.ColumnWidth + 1;
			using (var ptr = new QPainter(this)) {
				ptr.Translate(-doc.LeftColumn * doc.ColumnWidth, 0);
				DrawGrid(numRows, numCols, ptr);
				foreach (var row in doc.Rows) {
					int top = (row.Index - doc.TopRow) * doc.RowHeight;
					row.View.PaintContent(ptr, top, Width);
				}
				DrawCursor(ptr);
			}
		}

		private void DrawGrid(int numRows, int numCols, QPainter ptr)
		{
			ptr.FillRect(0, 0, numCols * doc.ColumnWidth, Height, GlobalColor.white);
			ptr.Pen = new QPen(GlobalColor.darkGray, 1, PenStyle.DotLine);
			var line = new QLine(0, 0, numCols * doc.ColumnWidth, 0);
			for (int i = 0; i <= numRows; i++) {
				ptr.DrawLine(line);
				line.Translate(0, doc.RowHeight);
			}
			ptr.Pen = new QPen(GlobalColor.darkGray, 1, PenStyle.DotLine);
			line = new QLine(0, 0, 0, Size.Height);
			for (int i = 0; i <= numCols / 5; i++) {
				ptr.DrawLine(line);
				line.Translate(doc.ColumnWidth * 5, 0);
			}
		}

		private QLine DrawCursor(QPainter ptr)
		{
			var line = new QLine();
			ptr.Pen = new QPen(GlobalColor.darkRed, 1);
			line = new QLine(0, 0, 0, Size.Height);
			line.Translate((int)(doc.ColumnWidth * (doc.CurrentColumn + 0.5)), 0);
			ptr.DrawLine(line);
			return line;
		}
	}
}
