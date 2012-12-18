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
		public static int MaxLinesPerRow = 5;

		int ActiveRow { get { return The.Timeline.ActiveRow; } set { The.Timeline.ActiveRow = value; } }
		int RowHeight { get { return The.Timeline.RowHeight; } set { The.Timeline.RowHeight = value; } }
		int ColWidth { get { return The.Timeline.ColWidth; } set { The.Timeline.ColWidth = value; } }

		public KeyGrid()
		{
			Paint += Keygrid_Paint;
		}

		void Keygrid_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			int numRows = Size.Height / RowHeight + 1;
			int numCols = Size.Width / ColWidth + 1;
			using (var ptr = new QPainter(this)) {
				DrawGrid(numRows, numCols, ptr);
				var nodes = The.Document.RootNode.Nodes;
				for (int i = 0; i < nodes.Count; i++) {
					var node = nodes[i];
					if (i < numRows) {
						DrawTransients(KeyTransientCollector.GetTransients(node), i, 0, numCols, ptr);
					}
				}
			}
		}

		private void DrawTransients(List<KeyTransient> keyMarks, int row, int startFrame, int endFrame, QPainter ptr)
		{
			for (int i = 0; i < keyMarks.Count; i++) {
				var m = keyMarks[i];
				if (m.Frame >= startFrame && m.Frame < endFrame) {
					int x = ColWidth * (m.Frame - startFrame) + 4;
					int y = RowHeight * row + m.Line * 6 + 4;
					DrawKey(ptr, m, x, y);
				}
			}
		}

		private void DrawKey(QPainter ptr, KeyTransient m, int x, int y)
		{
			ptr.FillRect(x - 3, y - 3, 6, 6, m.QColor);
			if (m.Length > 0) {
				int x1 = x + m.Length * ColWidth;
				ptr.FillRect(x, y - 1, x1 - x, 2, m.QColor);
			}
		}


		private void DrawGrid(int numRows, int numCols, QPainter ptr)
		{
			ptr.FillRect(Rect, GlobalColor.white);
			ptr.Pen = new QPen(GlobalColor.darkGray, 1, PenStyle.DotLine);
			var line = new QLine(0, 0, Size.Width, 0);
			for (int i = 0; i < numRows; i++) {
				line.Translate(0, RowHeight);
				ptr.DrawLine(line);
			}
			ptr.Pen = new QPen(GlobalColor.darkGray, 1, PenStyle.DotLine);
			line = new QLine(0, 0, 0, Size.Height);
			for (int i = 0; i < numCols / 5; i++) {
				line.Translate(ColWidth * 5, 0);
				ptr.DrawLine(line);
			}
		}
	}
}
