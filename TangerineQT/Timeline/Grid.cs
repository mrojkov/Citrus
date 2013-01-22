using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;
using System.Runtime.InteropServices;

namespace Tangerine.Timeline
{
	public class Grid : QWidget
	{
		Document doc { get { return The.Document; } }
		QTimer timer;

		public static int MaxLinesPerRow = 5;
		private GridState state = new GridIdleState();

		public Grid()
		{
			Paint += this_Paint;
			this.MouseMove += this_MouseMove;
			this.MousePress += this_MousePress;
			this.MouseRelease += this_MouseRelease;
			CreateScrollTimer();
		}

		public void ChangeState(GridState newState)
		{
			state = newState;
		}

		void this_MouseRelease(object sender, QEventArgs<QMouseEvent> e)
		{
			state.OnMouseRelease();
			timer.Stop();
		}

		void this_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
			state.OnMousePress(GetCellUnderMouseCursor());
			timer.Start();
		}

		void this_MouseMove(object sender, QEventArgs<QMouseEvent> e)
		{
			state.OnMouseMove(GetCellUnderMouseCursor());
		}

		private void CreateScrollTimer()
		{
			timer = new QTimer(this);
			timer.Interval = 50;
			timer.Timer += this_Timer;
		}

		void this_Timer(object sender, QEventArgs<QTimerEvent> e)
		{
			if (Toolbox.IsLeftButtonPressed()) {
				DoHorizontalScrolling();
				DoVerticalScrolling();
				state.OnMouseMove(GetCellUnderMouseCursor());
				doc.UpdateViews();
			}
		}

		Lime.IntVector2 GetCellUnderMouseCursor()
		{
			Lime.IntVector2 cell = Toolbox.PixelToCell(MapFromGlobal(QCursor.Pos));
			cell.Y = cell.Y.Clamp(0, doc.Rows.Count - 1);
			return cell;
		}

		private void DoVerticalScrolling()
		{
			int row = Toolbox.PixelToRow(MapFromGlobal(QCursor.Pos).Y);
			if (row <= doc.TopRow) {
				row = doc.TopRow - 1;
			} else if (row >= doc.BottomRow) {
				row = doc.BottomRow + 1;
			}
			The.Timeline.EnsureRowVisible(row);
		}

		private void DoHorizontalScrolling()
		{
			int column = Toolbox.PixelToColumn(MapFromGlobal(QCursor.Pos).X);
			if (column <= doc.LeftColumn) {
				column = doc.LeftColumn - 10;
			} else if (column >= doc.RightColumn) {
				column = doc.RightColumn + 10;
			}
			doc.CurrentColumn = Math.Max(0, column);
			The.Timeline.EnsureColumnVisible(column);
		}

		void this_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			int numRows = Size.Height / doc.RowHeight + 1;
			int numCols = Toolbox.CalcLastColumn(); // Size.Width / doc.ColumnWidth + 1;
			using (var ptr = new QPainter(this)) {
				ptr.Translate(-doc.LeftColumn * doc.ColumnWidth, 0);
				DrawGrid(numRows, numCols, ptr);
				foreach (var row in doc.Rows) {
					int top = (row.Index - doc.TopRow) * doc.RowHeight;
					row.View.PaintContent(ptr, top, Width);
				}
				state.Paint(ptr);
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
