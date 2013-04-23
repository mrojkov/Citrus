using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	class SexyVPaned : SexyPaned
	{
		public SexyVPaned()
		{
			this.MouseMoved += this_MouseMoved;
			this.BoundsChanged += this_BoundsChanged;
			this.ButtonPressed += this_ButtonPressed;
			this.ButtonReleased += this_ButtonReleased;
			this.MouseMoved += (a, e) => { Console.WriteLine("SexyVPaned moved {0}", e.X); };
		}

		protected override void RefreshMinExtension()
		{
			double h = GetPanelPreferredHeight(0) + GetPanelPreferredHeight(1);
			this.MinHeight = Math.Max(dragBandWidth, h);
		}

		double GetPanelPreferredHeight(int index)
		{
			var p = panels[index].Content;
			if (p != null) {
				p.Surface.Reallocate();
				return p.Surface.GetPreferredHeight().NaturalSize;
			} else {
				return 0;
			}
		}

		protected override void SetPosition(double value)
		{
			position = Math.Max(value, GetPanelPreferredHeight(0));
			position = Math.Min(position, Size.Height - GetPanelPreferredHeight(1));
			position = Math.Min(Math.Max(position, 0), Size.Height);
			position = Math.Round(position);
			if (Panel1.Content != null) {
				var b1 = new Xwt.Rectangle(0, 0, Size.Width, Position);
				this.SetChildBounds(Panel1.Content, b1);
			}
			if (Panel2.Content != null) {
				var b2 = new Xwt.Rectangle(0, Position + 1, Size.Width, Size.Height - Position - 1);
				this.SetChildBounds(Panel2.Content, b2);
			}
			RefreshMinExtension();
			this.QueueDraw();
		}

		void this_ButtonReleased(object sender, Xwt.ButtonEventArgs e)
		{
			if (dragging) {
				dragging = false;
				SetWindowCursor(Xwt.CursorType.Arrow);
			}
		}

		void this_ButtonPressed(object sender, Xwt.ButtonEventArgs e)
		{
			if (Math.Abs(e.Y - Position) < dragBandWidth) {
				dragging = true;
				e.Handled = true;
			}
		}

		bool overDragBand;

		void this_MouseMoved(object sender, Xwt.MouseMovedEventArgs e)
		{
			if (dragging) {
				Position = e.Y;
				e.Handled = true;
			} else {
				if (Math.Abs(e.Y - Position) < dragBandWidth) {
					e.Handled = true;
					overDragBand = true;
					SetWindowCursor(Xwt.CursorType.ResizeUpDown);
				} else if (overDragBand) {
					SetWindowCursor(Xwt.CursorType.Arrow);
					overDragBand = false;
				}
			}
		}

		Xwt.Size previousSize;

		void this_BoundsChanged(object sender, EventArgs e)
		{
			if (previousSize.IsZero || previousSize == new Xwt.Size(1, 1)) {
				if (Size != new Xwt.Size(1, 1)) {
					Position = Math.Round(Size.Height / 2);
					previousSize = Size;
				}
			} else {
				double p = Position / previousSize.Height;
				Position = Size.Height * p;
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
			ctx.MoveTo(0, Position);
			ctx.LineTo(this.Size.Width, Position);
			ctx.Stroke();
		}
	}
}
