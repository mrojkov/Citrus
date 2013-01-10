using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class KeyGridRuler : QWidget
	{
		Document doc { get { return The.Document; } }
		CachedTextPainter textPainter = new CachedTextPainter();

		public KeyGridRuler()
		{
			this.SetFixedHeight(doc.RowHeight);
			Paint += KeyGridRuler_Paint;
		}

		void KeyGridRuler_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			int numCols = TimelineToolbox.MaxColumn; // Size.Width / doc.ColumnWidth + 1;
			using (var ptr = new QPainter(this)) {
				ptr.Translate(-doc.LeftColumn * doc.ColumnWidth, 0);
				DrawGrid(numCols, ptr);
			}
		}

		private void DrawGrid(int numCols, QPainter ptr)
		{
			// Рисуем засечки
			ptr.Pen = new QPen(GlobalColor.darkGray);
			var line = new QLine(0, Size.Height - 6, 0, Size.Height - 5);
			for (int i = 0; i <= numCols; i++) {
				line.Translate(doc.ColumnWidth, 0);
				ptr.DrawLine(line);
			}
			// Рисуем курсор
			ptr.Pen = new QPen(GlobalColor.darkRed, 1);
			ptr.Brush = new QBrush(new QColor(255, 128, 128));
			ptr.DrawRect(doc.CurrentColumn * doc.ColumnWidth, 1, doc.ColumnWidth, Height - 2);
			// Рисуем числа
			ptr.Pen = new QPen(GlobalColor.black);
			ptr.Font = new QFont("Tahoma", 9);
			for (int i = 0; i <= numCols / 10; i++) {
				textPainter.Draw(ptr, i * doc.ColumnWidth * 10 + 1, 5, (i * 10).ToString());
			}
		}
	}
}
