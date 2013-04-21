using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	class DragSnippet : Xwt.Window
	{
		Xwt.Label titleLabel;
		CustomCanvas canvas;

		public DragSnippet(string title)
		{
			CreateContent(title);
			UpdateLocation();
			Xwt.Application.TimeoutInvoke(10, DragHandler);
		}

		private void CreateContent(string title)
		{
			canvas = new CustomCanvas();
			this.Padding = new Xwt.WidgetSpacing();
			this.TransientFor = MainWindow.Instance.Window;
			this.Size = new Xwt.Size(100, 50);
			this.Decorated = false;
			this.Content = canvas;
			canvas.BoundsChanged += canvas_BoundsChanged;
			titleLabel = new Xwt.Label(title);
			titleLabel.TextAlignment = Xwt.Alignment.Center;
			canvas.AddChild(titleLabel);
			canvas.Drawn += canvas_Drawn;
			this.Show();
		}

		void canvas_Drawn(Xwt.Drawing.Context ctx, Xwt.Rectangle dirtyRect)
		{
			ctx.SetColor(Xwt.Drawing.Colors.White);
			ctx.Rectangle(canvas.Bounds);
			ctx.FillPreserve();
			ctx.SetColor(Xwt.Drawing.Colors.Black);
			ctx.Stroke();
		}

		void canvas_BoundsChanged(object sender, EventArgs e)
		{
			canvas.SetChildBounds(titleLabel, canvas.Bounds);
		}

		private bool DragHandler()
		{
			UpdateLocation();
			if (!XwtPlus.Utils.Instance.GetPointerButtonState(Xwt.PointerButton.Left)) {
				Hide();
				return false;
			}
			return true;
		}

		private void UpdateLocation()
		{
			Xwt.Point p = XwtPlus.Utils.Instance.GetPointerPosition();
			this.Location = p - new Xwt.Size(Size.Width / 2, Size.Height / 2);
		}
	}
}
