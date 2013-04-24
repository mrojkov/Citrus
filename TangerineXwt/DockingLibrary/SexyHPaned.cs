using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
#if SEXY_PANED
	public class SexyHPaned : SexyPaned
	{
		public SexyHPaned()
		{
			this.MouseMoved += this_MouseMoved;
			this.BoundsChanged += this_BoundsChanged;
			this.ButtonPressed += this_ButtonPressed;
			this.ButtonReleased += this_ButtonReleased;
		}

		protected override void RefreshMinSize()
		{
			double w = GetPanelMinWidth(0) + GetPanelMinWidth(1);
			this.MinHeight = Math.Max(dragBandWidth, w);
		}

		private double ClampPosition(double position)
		{
			double result = Math.Max(position, GetPanelMinWidth(0));
			result = Math.Min(result, Size.Width - GetPanelMinWidth(1));
			result = Math.Min(Math.Max(result, 0), Size.Width);
			result = Math.Round(result);
			return result;
		}

		double GetPanelMinWidth(int index)
		{
			var p = panels[index].Content;
			if (p != null) {
				p.Surface.Reallocate();
				return p.Surface.GetPreferredWidth().MinSize;
			} else {
				return 0;
			}
		}

		protected override void SetPosition(double value)
		{
			position = value;
			if (Panel1.Content != null) {
				var b1 = new Xwt.Rectangle(0, 0, ClampPosition(value), Size.Height);
				this.SetChildBounds(Panel1.Content, b1);
			}
			if (Panel2.Content != null) {
				var b2 = new Xwt.Rectangle(ClampPosition(Position) + 1, 0, Size.Width - ClampPosition(value) - 1, Size.Height);
				this.SetChildBounds(Panel2.Content, b2);
			}
			RefreshMinSize();
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
			if (Math.Abs(e.X - ClampPosition(Position)) < dragBandWidth) {
				dragging = true;
				e.Handled = true;
			}
		}

		bool overDragBand;

		void this_MouseMoved(object sender, Xwt.MouseMovedEventArgs e)
		{
			if (!XwtPlus.Utils.Instance.GetPointerButtonState(Xwt.PointerButton.Left)) {
				dragging = false;
			}
			if (dragging) {
				Position = ClampPosition(e.X);
				e.Handled = true;
			} else {
				if (Math.Abs(e.X - ClampPosition(Position)) < dragBandWidth) {
					e.Handled = true;
					overDragBand = true;
					SetWindowCursor(Xwt.CursorType.ResizeLeftRight);
				} else if (overDragBand) {
					overDragBand = false;
					SetWindowCursor(Xwt.CursorType.Arrow);
				}
			}
		}

		Xwt.Size lastSize;

		void this_BoundsChanged(object sender, EventArgs e)
		{
			if (lastSize.IsZero || lastSize == new Xwt.Size(1, 1)) {
				if (Size != new Xwt.Size(1, 1)) {
					Position = Math.Round(Size.Width / 2);
					lastSize = Size;
				}
			} else {
				double p = Position / lastSize.Width;
				Position = Size.Width * p;
				lastSize = Size;
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
			ctx.MoveTo(ClampPosition(position), 0);
			ctx.LineTo(ClampPosition(position), this.Size.Height);
			ctx.Stroke();
		}
	}
#else
	public class SexyHPaned : Xwt.HPaned
	{
	}
#endif
}
