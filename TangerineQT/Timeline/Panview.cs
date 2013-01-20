using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine.Timeline
{
	public class Panview : QWidget
	{
		bool dragging;
		int leftColumnBeforeDrag;
		int topRowBeforeDrag;
		int maxColumnBeforeDrag;
		QPoint dragPosition;

		Document doc { get { return The.Document; } }

		public Panview()
		{
			Paint += this_Paint;
			this.MousePress += this_MousePress;
			this.MouseRelease += this_MouseRelease;
			this.MouseMove += this_MouseMove;
		}

		void this_MouseMove(object sender, QEventArgs<QMouseEvent> e)
		{
			if (dragging) {
				doc.LeftColumn = PixelToColumn(e.Event.Pos().X - dragPosition.X) + leftColumnBeforeDrag;
				doc.LeftColumn = Math.Min(doc.LeftColumn, maxColumnBeforeDrag - Toolbox.CalcNumberOfVisibleColumns());
				doc.LeftColumn = Math.Max(doc.LeftColumn, 0);
				doc.TopRow = PixelToRow(e.Event.Pos().Y - dragPosition.Y) + topRowBeforeDrag;
				doc.TopRow = Math.Min(doc.TopRow, doc.Rows.Count - Toolbox.CalcNumberOfVisibleRows());
 				doc.TopRow = Math.Max(doc.TopRow, 0);
				doc.UpdateViews();
			}
		}

		int PixelToColumn(int x)
		{
			return (int)(x / (GetCanvasScale().X * doc.ColumnWidth) + 0.5f);
		}

		int PixelToRow(int y)
		{
			return (int)(y / (GetCanvasScale().Y * doc.RowHeight) + 0.5f);
		}

		void this_MouseRelease(object sender, QEventArgs<QMouseEvent> e)
		{
			dragging = false;
			Repaint();
		}

		void this_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
			dragging = true;
			dragPosition = e.Event.Pos();
			leftColumnBeforeDrag = doc.LeftColumn;
			topRowBeforeDrag = doc.TopRow;
			maxColumnBeforeDrag = Toolbox.CalcLastColumn();
		}

		void this_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			if (doc.Rows.Count > 0) {
				using (var ptr = new QPainter(this)) {
					SetScale(ptr);
					PaintVisibleRect(ptr);
					PaintRows(ptr);
				}
			}
		}

		private void PaintVisibleRect(QPainter ptr)
		{
			int numCols = Toolbox.CalcNumberOfVisibleColumns();
			int numRows = Math.Min(Toolbox.CalcNumberOfVisibleRows(), doc.Rows.Count - doc.TopRow);
			int x = doc.LeftColumn * doc.ColumnWidth;
			int y = doc.TopRow * doc.RowHeight;
			int w = numCols * doc.ColumnWidth;
			int h = numRows * doc.RowHeight;
			ptr.Pen = new QPen(GlobalColor.black);
			ptr.Brush = new QBrush(GlobalColor.white);
			ptr.DrawRect(x, y, w, h);
		}

		private void SetScale(QPainter ptr)
		{
			ptr.Scale(GetCanvasScale().X, GetCanvasScale().Y);
		}

		private Lime.Vector2 GetCanvasScale()
		{
			int width = Toolbox.CalcLastColumn() * doc.ColumnWidth;
			int height = doc.Rows.Count * doc.RowHeight;
			Lime.Vector2 scale;
			scale.X = (Width - 1) / (float)width;
			scale.Y = (Height - 1) / (float)height;
			return scale;
		}

		private void PaintRows(QPainter ptr)
		{
			int width = Toolbox.CalcLastColumn() * doc.ColumnWidth;
			foreach (var row in doc.Rows) {
				int top = row.Index * doc.RowHeight;
				row.View.PaintContent(ptr, top, width);
			}
		}
	}
}
