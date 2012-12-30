using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class KeyGridRuler : QToolBar
	{
		CachedTextPainter textPainter = new CachedTextPainter();

		public KeyGridRuler()
		{
			this.SetFixedHeight(RowHeight);
			Paint += KeyGridRuler_Paint;
		}

		int RowHeight { get { return The.Preferences.TimelineRowHeight; } }
		int ColWidth { get { return The.Preferences.TimelineColWidth; } }

		void KeyGridRuler_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			int numCols = Size.Width / ColWidth + 1;
			using (var ptr = new QPainter(this)) {
				DrawGrid(numCols, ptr);
			}
		}

		private void DrawGrid(int numCols, QPainter ptr)
		{
			// Рисуем засечки
			ptr.Pen = new QPen(GlobalColor.darkGray);
			var line = new QLine(0, Size.Height - 6, 0, Size.Height - 5);
			for (int i = 0; i <= numCols; i++) {
				line.Translate(ColWidth, 0);
				ptr.DrawLine(line);
			}
			// Рисуем числа
			ptr.Pen = new QPen(GlobalColor.black);
			ptr.Font = new QFont("Tahoma", 9);
			for (int i = 0; i <= numCols / 10; i++) {
				textPainter.Draw(ptr, i * ColWidth * 10 + 1, 5, (i * 10).ToString());
			}
		}
	}
}
