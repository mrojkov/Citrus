using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine.Timeline
{
	public class TimlineRuler : QWidget
	{
		QTimer timer;
		Document doc { get { return The.Document; } }
		CachedTextPainter textPainter = new CachedTextPainter();

		public TimlineRuler()
		{
			this.SetFixedHeight(doc.RowHeight);
			Paint += this_Paint;
			this.MouseMove += this_MouseMove;
			this.MousePress += this_MousePress;
			this.MouseRelease += this_MouseRelease;
			CreateScrollTimer();
		}

		void this_MouseRelease(object sender, QEventArgs<QMouseEvent> e)
		{
			timer.Stop();
		}

		private void CreateScrollTimer()
		{
			timer = new QTimer(this);
			timer.Interval = 10;
			timer.Timer += this_Timer;
		}

		void this_Timer(object sender, QEventArgs<QTimerEvent> e)
		{
			if (IsLeftButtonPressed()) {
				int column = Toolbox.PixelToColumn(MapFromGlobal(QCursor.Pos).X);
				if (column <= doc.LeftColumn) {
					column = doc.LeftColumn - 10;
				} else if (column >= doc.RightColumn) {
					column = doc.RightColumn + 10;
				}
				Toolbox.SetCurrentColumn(column);
			}
		}

		private static bool IsLeftButtonPressed()
		{
			return (QApplication.MouseButtons() & MouseButton.LeftButton) != 0;
		}

		void this_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
			int column = Toolbox.PixelToColumn(e.Event.Pos().X);
			timer.Start();
			Toolbox.SetCurrentColumn(column);
		}

		void this_MouseMove(object sender, QEventArgs<QMouseEvent> e)
		{
			if (IsLeftButtonPressed()) {
				int column = Toolbox.PixelToColumn(e.Event.Pos().X);
				column = column.Clamp(doc.LeftColumn, doc.RightColumn);
				Toolbox.SetCurrentColumn(column);
			}
		}

		void this_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			int numCols = Toolbox.CalcLastColumn();
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
