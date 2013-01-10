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
		Document doc { get { return The.Document; } }

		bool dragging;

		public Panview()
		{
			//this.Palette = new QPalette(new QColor(230, 230, 230));
			//this.AutoFillBackground = true;
			Paint += Panview_Paint;
			this.MousePress += Panview_MousePress;
			this.MouseRelease += Panview_MouseRelease;
			this.MouseMove += Panview_MouseMove;
		}

		void Panview_MouseMove(object sender, QEventArgs<QMouseEvent> e)
		{
			if (dragging) {
			
			}
		}

		void Panview_MouseRelease(object sender, QEventArgs<QMouseEvent> e)
		{
			dragging = false;
		}

		void Panview_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
			dragging = true;
		}

		void Panview_Paint(object sender, QEventArgs<QPaintEvent> e)
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
			int numCols = TimelineToolbox.NumberOfVisibleColumns;
			int numRows = Math.Min(TimelineToolbox.NumberOfVisibleRows, doc.Rows.Count - doc.TopRow);
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
			int width = TimelineToolbox.MaxColumn * doc.ColumnWidth;
			int height = doc.Rows.Count * doc.RowHeight;
			ptr.Scale((Width - 1) / (double)width, (Height - 1) / (double)height);
		}

		private void PaintRows(QPainter ptr)
		{
			int width = TimelineToolbox.MaxColumn * doc.ColumnWidth;
			foreach (var row in doc.Rows) {
				int top = row.Index * doc.RowHeight;
				row.View.PaintContent(ptr, top, width);
			}
		}
	}
}
