using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Timeline
{
	public class Panview
	{
		public CustomCanvas Canvas { get; private set; }
		public double Width { get { return Canvas.Size.Width; } }
		public double Height { get { return Canvas.Size.Height; } }

		Document doc { get { return The.Document; } }

		bool dragging;
		int leftColumnBeforeDrag;
		int topRowBeforeDrag;
		int maxColumnBeforeDrag;
		// QPoint dragPosition;

		public Panview()
		{
			Canvas = new CustomCanvas();
			Canvas.Drawn += Panview_Drawn;
			//this.MousePress += this_MousePress;
			//this.MouseRelease += this_MouseRelease;
			//this.MouseMove += this_MouseMove;
		}

		//void this_MouseMove(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	if (dragging) {
		//		doc.LeftColumn = PixelToColumn(e.Event.Pos().X - dragPosition.X) + leftColumnBeforeDrag;
		//		doc.LeftColumn = Math.Min(doc.LeftColumn, maxColumnBeforeDrag - Toolbox.CalcNumberOfVisibleColumns());
		//		doc.LeftColumn = Math.Max(doc.LeftColumn, 0);
		//		doc.TopRow = PixelToRow(e.Event.Pos().Y - dragPosition.Y) + topRowBeforeDrag;
		//		doc.TopRow = Math.Min(doc.TopRow, doc.Rows.Count - Toolbox.CalcNumberOfVisibleRows());
		//		doc.TopRow = Math.Max(doc.TopRow, 0);
		//		doc.UpdateViews();
		//	}
		//}

		int PixelToColumn(double x)
		{
			return (int)(x / (GetCanvasScale().X * doc.ColumnWidth) + 0.5f);
		}

		int PixelToRow(double y)
		{
			return (int)(y / (GetCanvasScale().Y * doc.RowHeight) + 0.5f);
		}

		//void this_MouseRelease(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	dragging = false;
		//	Repaint();
		//}

		//void this_MousePress(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	dragging = true;
		//	dragPosition = e.Event.Pos();
		//	leftColumnBeforeDrag = doc.LeftColumn;
		//	topRowBeforeDrag = doc.TopRow;
		//	maxColumnBeforeDrag = Toolbox.CalcLastColumn();
		//}

		void Panview_Drawn(Xwt.Drawing.Context ctx, Xwt.Rectangle dirtyRect)
		{
			if (doc.Rows.Count > 0) {
				PaintBackground(ctx);
				SetScale(ctx);
				PaintVisibleRect(ctx);
				PaintRows(ctx);
			}
		}

		private void PaintBackground(Xwt.Drawing.Context ctx)
		{
			ctx.Rectangle(0, 0, Width, Height);
			ctx.SetColor(Colors.GridLines);
			ctx.Fill();
		}

		private void PaintVisibleRect(Xwt.Drawing.Context ctx)
		{
			int numCols = Toolbox.CalcNumberOfVisibleColumns();
			int numRows = Math.Min(Toolbox.CalcNumberOfVisibleRows(), doc.Rows.Count - doc.TopRow);
			double x = doc.LeftColumn * doc.ColumnWidth;
			double y = doc.TopRow * doc.RowHeight;
			double w = numCols * doc.ColumnWidth;
			double h = numRows * doc.RowHeight;
			ctx.SetColor(Colors.ActiveBackground);
			ctx.Rectangle(x, y, w, h);
			ctx.Fill();
			//ctx.SetColor(Colors.GridLines);
			//ctx.Stroke();
		}

		private void SetScale(Xwt.Drawing.Context ctx)
		{
			ctx.Scale(GetCanvasScale().X, GetCanvasScale().Y);
		}

		private Xwt.Point GetCanvasScale()
		{
			double width = Toolbox.CalcLastColumn() * doc.ColumnWidth;
			double height = doc.Rows.Count * doc.RowHeight;
			var scale = new Xwt.Point {
				X = (Width - 1) / width,
				Y = (Height - 1) / height
			};
			return scale;
		}

		private void PaintRows(Xwt.Drawing.Context ctx)
		{
			KeyTransientsPainter.DrawCounter = 0;
			double width = Toolbox.CalcLastColumn() * doc.ColumnWidth;
			foreach (var row in doc.Rows) {
				double top = row.Index * doc.RowHeight;
				row.View.PaintContent(ctx, top, width);
			}
			//Console.WriteLine(KeyTransientsPainter.DrawCounter);
		}
	}
}
