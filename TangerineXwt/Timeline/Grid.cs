using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Tangerine.Timeline
{
	public class Grid
	{
		public CustomCanvas Canvas { get; private set; }
		Document doc { get { return The.Document; } }
		//QTimer timer;

		public double Width { get { return Canvas.Size.Width; } }
		public double Height { get { return Canvas.Size.Height; } }

		public static int MaxLinesPerRow = 5;
		//private GridState state = new GridIdleState();

		public Grid()
		{
			Canvas = new CustomCanvas();
			Canvas.Drawn += Grid_Drawn;
			//Paint += this_Paint;
			//this.MouseMove += this_MouseMove;
			//this.MousePress += this_MousePress;
			//this.MouseRelease += this_MouseRelease;
			//CreateScrollTimer();
		}

		void Grid_Drawn(Xwt.Drawing.Context ctx, Xwt.Rectangle dirtyRect)
		{
			int numRows = (int)(Height / doc.RowHeight) + 1;
			int numCols = Toolbox.CalcLastColumn(); // Size.Width / doc.ColumnWidth + 1;
			ctx.SetLineWidth(1);
			ctx.Translate(-doc.LeftColumn * doc.ColumnWidth + 0.5, 0.5);
			DrawGrid(numRows, numCols, ctx);
			foreach (var row in doc.Rows) {
				double top = (row.Index - doc.TopRow) * doc.RowHeight;
				row.View.PaintContent(ctx, top, Width);
			}
		}

		private void DrawGrid(int numRows, int numCols, Xwt.Drawing.Context ctx)
		{
			ctx.SetColor(Colors.ActiveBackground);
			ctx.Rectangle(0, 0, numCols * doc.ColumnWidth, Height);
			ctx.Fill();
			ctx.SetColor(Colors.GridLines);
			for (int i = 0; i <= numRows; i++) {
				ctx.MoveTo(0, doc.RowHeight * i);
				ctx.LineTo(numCols * doc.ColumnWidth, doc.RowHeight * i);
			}
			for (int i = 0; i <= numCols / 5; i++) {
				ctx.MoveTo(doc.ColumnWidth * i * 5, 0);
				ctx.LineTo(doc.ColumnWidth * i * 5, Height);
			}
			ctx.Stroke();
			DrawCursor(ctx);
		}

		private void DrawCursor(Xwt.Drawing.Context ctx)
		{
			double x = doc.ColumnWidth * (doc.CurrentColumn + 0.5);
			ctx.MoveTo(x, 0);
			ctx.LineTo(x, Height);
			ctx.SetColor(Colors.TimelineCursor);
			ctx.Stroke();
		}

		//public void ChangeState(GridState newState)
		//{
		//	state = newState;
		//}

		//void this_MouseRelease(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	state.OnMouseRelease();
		//	timer.Stop();
		//}

		//void this_MousePress(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	state.OnMousePress(GetCellUnderMouseCursor());
		//	timer.Start();
		//}

		//void this_MouseMove(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	state.OnMouseMove(GetCellUnderMouseCursor());
		//}

		//private void CreateScrollTimer()
		//{
		//	timer = new QTimer(this);
		//	timer.Interval = 50;
		//	timer.Timer += this_Timer;
		//}

		//void this_Timer(object sender, QEventArgs<QTimerEvent> e)
		//{
		//	if (Toolbox.IsLeftButtonPressed()) {
		//		DoHorizontalScrolling();
		//		DoVerticalScrolling();
		//		state.OnMouseMove(GetCellUnderMouseCursor());
		//		doc.UpdateViews();
		//	}
		//}

		//Lime.IntVector2 GetCellUnderMouseCursor()
		//{
		//	Lime.IntVector2 cell = Toolbox.PixelToCell(MapFromGlobal(QCursor.Pos));
		//	cell.Y = cell.Y.Clamp(0, doc.Rows.Count - 1);
		//	return cell;
		//}

		//private void DoVerticalScrolling()
		//{
		//	int row = Toolbox.PixelToRow(MapFromGlobal(QCursor.Pos).Y);
		//	if (row <= doc.TopRow) {
		//		row = doc.TopRow - 1;
		//	} else if (row >= doc.BottomRow) {
		//		row = doc.BottomRow + 1;
		//	}
		//	The.Timeline.EnsureRowVisible(row);
		//}

		//private void DoHorizontalScrolling()
		//{
		//	int column = Toolbox.PixelToColumn(MapFromGlobal(QCursor.Pos).X);
		//	if (column <= doc.LeftColumn) {
		//		column = doc.LeftColumn - 10;
		//	} else if (column >= doc.RightColumn) {
		//		column = doc.RightColumn + 10;
		//	}
		//	doc.CurrentColumn = Math.Max(0, column);
		//	The.Timeline.EnsureColumnVisible(column);
		//}

		//void this_Paint(object sender, QEventArgs<QPaintEvent> e)
		//{
		//	int numRows = Size.Height / doc.RowHeight + 1;
		//	int numCols = Toolbox.CalcLastColumn(); // Size.Width / doc.ColumnWidth + 1;
		//	using (var ptr = new QPainter(this)) {
		//		ptr.Translate(-doc.LeftColumn * doc.ColumnWidth, 0);
		//		DrawGrid(numRows, numCols, ptr);
		//		foreach (var row in doc.Rows) {
		//			int top = (row.Index - doc.TopRow) * doc.RowHeight;
		//			row.View.PaintContent(ptr, top, Width);
		//		}
		//		state.Paint(ptr);
		//		DrawCursor(ptr);
		//	}
		//}
	}
}
