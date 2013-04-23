using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public class SexyHPaned : SexyPaned
	{
		public SexyHPaned()
		{
			this.MouseMoved += this_MouseMoved;
			this.BoundsChanged += this_BoundsChanged;
			this.ButtonPressed += this_ButtonPressed;
			this.ButtonReleased += this_ButtonReleased;
		}

		protected override void RefreshMinExtension()
		{
			double w = GetPanelPreferredWidth(0) + GetPanelPreferredWidth(1);
			this.MinHeight = Math.Max(dragBandWidth, w);
		}

		double GetPanelPreferredWidth(int index)
		{
			var p = panels[index].Content;
			if (p != null) {
				p.Surface.Reallocate();
				return p.Surface.GetPreferredWidth().NaturalSize;
			} else {
				return 0;
			}
		}

		protected override void SetPosition(double value)
		{
			position = Math.Max(value, GetPanelPreferredWidth(0));
			position = Math.Min(position, Size.Width - GetPanelPreferredWidth(1));
			position = Math.Min(Math.Max(position, 0), Size.Width);
			position = Math.Round(position);
			if (Panel1.Content != null) {
				var b1 = new Xwt.Rectangle(0, 0, Position, Size.Height);
				this.SetChildBounds(Panel1.Content, b1);
			}
			if (Panel2.Content != null) {
				var b2 = new Xwt.Rectangle(Position + 1, 0, Size.Width - Position - 1, Size.Height);
				this.SetChildBounds(Panel2.Content, b2);
			}
			RefreshMinExtension();
			this.QueueDraw();
		}

		void this_ButtonReleased(object sender, Xwt.ButtonEventArgs e)
		{
			if (dragging) {
				dragging = false;
				MainWindow.Instance.Window.Content.Cursor = Xwt.CursorType.Arrow;
			}
		}

		void this_ButtonPressed(object sender, Xwt.ButtonEventArgs e)
		{
			if (Math.Abs(e.X - Position) < dragBandWidth) {
				dragging = true;
				e.Handled = true;
			}
		}

		bool overDragBand;

		void this_MouseMoved(object sender, Xwt.MouseMovedEventArgs e)
		{
			if (dragging) {
				Position = e.X;
				e.Handled = true;
			} else {
				if (Math.Abs(e.X - Position) < dragBandWidth) {
					e.Handled = true;
					overDragBand = true;
					SetWindowCursor(Xwt.CursorType.ResizeLeftRight);
				} else if (overDragBand) {
					overDragBand = false;
					SetWindowCursor(Xwt.CursorType.Arrow);
				}
			}
		}

		Xwt.Size previousSize;

		void this_BoundsChanged(object sender, EventArgs e)
		{
			if (previousSize.IsZero || previousSize == new Xwt.Size(1, 1)) {
				if (Size != new Xwt.Size(1, 1)) {
					Position = Math.Round(Size.Width / 2);
					previousSize = Size;
				}
			} else {
				double p = Position / previousSize.Width;
				Position = Size.Width * p;
				previousSize = Size;
			}
		}

		protected override void OnDraw(Xwt.Drawing.Context ctx, Xwt.Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);
			DrawDividingLine(ctx);
		}

		private void DrawDividingLine(Xwt.Drawing.Context ctx)
		{
			ctx.Translate(0.5, 0.5);
			ctx.SetLineWidth(1);
			ctx.SetColor(Colors.SexySash);
			ctx.MoveTo(Position, 0);
			ctx.LineTo(Position, this.Size.Height);
			ctx.Stroke();
		}
	}
}
