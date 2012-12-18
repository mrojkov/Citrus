using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class Panview : QWidget
	{
		int RowHeight, ColWidth;

		public Panview()
		{
			Paint += Panview_Paint;
		}

		void Panview_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			var container = The.Document.RootNode;
			ColWidth = 4;
			RowHeight = 3;// Height / container.Nodes.Count;
			int numRows = Height / RowHeight + 1;
			int numCols = Width / ColWidth + 1;
			using (var ptr = new QPainter(this)) {
				//DrawGrid(numRows, numCols, ptr);
				for (int i = 0; i < container.Nodes.Count; i++) {
					var node = container.Nodes[i];
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
			ptr.FillRect(x - 1, y - 1, 2, 2, m.QColor);
			if (m.Length > 0) {
				int x1 = x + m.Length * ColWidth;
				ptr.FillRect(x, y, x1 - x, 1, m.QColor);
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
