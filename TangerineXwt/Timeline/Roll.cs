using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Timeline
{
	public class Roll
	{
		public CustomCanvas Canvas { get; private set; }
		public double Width { get { return Canvas.Size.Width; } }
		public double Height { get { return Canvas.Size.Height; } }

		Document doc { get { return The.Document; } }
		
		public Roll()
		{
			Canvas = new CustomCanvas();
			Canvas.Drawn += Roll_Drawn;
			Canvas.ButtonPressed += Roll_ButtonPressed;
			Canvas.BoundsChanged += Roll_BoundsChanged;
		}

		void Roll_Drawn(Xwt.Drawing.Context ctx, Xwt.Rectangle dirtyRect)
		{
			ctx.SetColor(Colors.ActiveBackground);
			ctx.Rectangle(0, 0, Width, Height);
			ctx.Fill();
		}

		void Roll_BoundsChanged(object sender, EventArgs e)
		{
			if (The.Timeline != null) {
				The.Timeline.Refresh();
			}
		}

		void Roll_ButtonPressed(object sender, Xwt.ButtonEventArgs e)
		{
			int row = (int)(e.Y / doc.RowHeight) + doc.TopRow;
			if (row >= 0 && row < doc.Rows.Count) {
				var view = The.Document.Rows[row].View;
				if (e.MultiplePress == 1) {
					view.HandleClick(e);
				} else if (e.MultiplePress == 2) {
					view.HandleDoubleClick(e);
				}
			}
		}

		//protected override Xwt.WidgetSize OnGetPreferredHeight()
		//{
		//	return new Xwt.WidgetSize(The.Preferences.TimelineDefaultHeight);
		//}

		//protected override Xwt.WidgetSize OnGetPreferredWidth()
		//{
		//	return new Xwt.WidgetSize(The.Preferences.TimelineDefaultRollWidth);
		//}
	}
}
