using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Timeline
{
	public class Ruler
	{
		public CustomCanvas Canvas { get; private set; }
		public double Width { get { return Canvas.Size.Width; } }
		public double Height { get { return Canvas.Size.Height; } }

		// QTimer timer;
		Document doc { get { return The.Document; } }

		public Ruler()
		{
			Canvas = new CustomCanvas();
			Canvas.MinHeight = doc.RowHeight;
			Canvas.Drawn += Canvas_Drawn;
			//this.MouseMove += this_MouseMove;
			//this.MousePress += this_MousePress;
			//this.MouseRelease += this_MouseRelease;
			//CreateScrollTimer();
		}

		//void this_MouseRelease(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	timer.Stop();
		//}

		//private void CreateScrollTimer()
		//{
		//	timer = new QTimer(this);
		//	timer.Interval = 50;
		//	timer.Timer += this_Timer;
		//}

		//void this_Timer(object sender, QEventArgs<QTimerEvent> e)
		//{
		//	if (IsLeftButtonPressed()) {
		//		int column = Toolbox.PixelToColumn(MapFromGlobal(QCursor.Pos).X);
		//		if (column <= doc.LeftColumn) {
		//			column = doc.LeftColumn - 10;
		//		} else if (column >= doc.RightColumn) {
		//			column = doc.RightColumn + 10;
		//		}
		//		SetCurrentColumn(column);
		//	}
		//}

		//private void SetCurrentColumn(int column)
		//{
		//	doc.CurrentColumn = Math.Max(0, column);
		//	The.Timeline.EnsureColumnVisible(column);
		//	doc.UpdateViews();
		//}

		//private static bool IsLeftButtonPressed()
		//{
		//	return (QApplication.MouseButtons() & MouseButton.LeftButton) != 0;
		//}

		//void this_MousePress(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	int column = Toolbox.PixelToColumn(e.Event.Pos().X);
		//	timer.Start();
		//	SetCurrentColumn(column);
		//}

		//void this_MouseMove(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	if (IsLeftButtonPressed()) {
		//		int column = Toolbox.PixelToColumn(e.Event.Pos().X);
		//		column = column.Clamp(doc.LeftColumn, doc.RightColumn);
		//		SetCurrentColumn(column);
		//	}
		//}

		void Canvas_Drawn(Xwt.Drawing.Context ctx, Xwt.Rectangle dirtyRect)
		{
			int numCols = Toolbox.CalcLastColumn();
			ctx.Translate(-doc.LeftColumn * doc.ColumnWidth + 0.5, 0.5);
			DrawGrid(numCols, ctx);
		}

		private void DrawGrid(int numCols, Xwt.Drawing.Context ctx)
		{
			// Рисуем засечки
			ctx.SetLineWidth(1);
			ctx.SetColor(Colors.GridLines.WithIncreasedLight(-0.1));
			for (int i = 0; i <= numCols; i++) {
				ctx.MoveTo(i * doc.ColumnWidth, Height - 4);
				ctx.LineTo(i * doc.ColumnWidth, Height - 2);
			}
			ctx.Stroke();
			// Рисуем курсор
			ctx.Rectangle(doc.CurrentColumn * doc.ColumnWidth, 1, doc.ColumnWidth, Height - 1);
			ctx.SetColor(Colors.TimelineCursor);
			ctx.FillPreserve();
			ctx.SetColor(Colors.TimelineCursor.WithIncreasedLight(-0.3));
			//ctx.Stroke();
			// Рисуем числа
			ctx.SetColor(Colors.Text);
			for (int i = 0; i <= numCols / 10; i++) {
				var l = new Xwt.Drawing.TextLayout() { Text = (i * 10).ToString() };
				ctx.DrawTextLayout(l, i * doc.ColumnWidth * 10, 2);
			}
		}
	}
}
