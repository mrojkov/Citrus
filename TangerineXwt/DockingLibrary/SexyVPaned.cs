using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
#if SEXY_PANED
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

		protected override void RefreshMinSize()
		{
			double h = GetPanelMinHeight(0) + GetPanelMinHeight(1);
			this.MinHeight = Math.Max(dragBandWidth, h);
		}

		private double ClampPosition(double position)
		{
			double result = Math.Max(position, GetPanelMinHeight(0));
			result = Math.Min(result, Size.Height - GetPanelMinHeight(1));
			result = Math.Min(Math.Max(result, 0), Size.Height);
			result = Math.Round(result);
			return result;
		}

		double GetPanelMinHeight(int index)
		{
			var p = panels[index].Content;
			if (p != null) {
				p.Surface.Reallocate();
				return p.Surface.GetPreferredHeight().MinSize;
			} else {
				return 0;
			}
		}

		protected override void SetPosition(double value)
		{
			position = value;
			if (Panel1.Content != null) {
				var b1 = new Xwt.Rectangle(0, 0, Size.Width, ClampPosition(value));
				this.SetChildBounds(Panel1.Content, b1);
			}
			if (Panel2.Content != null) {
				var b2 = new Xwt.Rectangle(0, ClampPosition(value) + 1, Size.Width, Size.Height - ClampPosition(value) - 1);
				this.SetChildBounds(Panel2.Content, b2);
			}
			RefreshMinSize();
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
			if (Math.Abs(e.Y - ClampPosition(Position)) < dragBandWidth) {
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
				Position = ClampPosition(e.Y);
				e.Handled = true;
			} else {
				if (Math.Abs(e.Y - ClampPosition(Position)) < dragBandWidth) {
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
			ctx.MoveTo(0, ClampPosition(position));
			ctx.LineTo(this.Size.Width, ClampPosition(position));
			ctx.Stroke();
		}
	}
#else
	public class SexyVPaned : Xwt.VPaned
	{
	}
#endif
}
