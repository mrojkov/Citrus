using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Timeline
{
	public class KeyTransientsPainter
	{
		public static int DrawCounter;

		double colWidth;
		double top;

		public KeyTransientsPainter(double colWidth, double top)
		{
			this.colWidth = colWidth;
			this.top = top;
		}

		public void DrawTransients(List<KeyTransient> transients, Xwt.Drawing.Context ptr)
		{
			for (int i = 0; i < transients.Count; i++) {
				var m = transients[i];
				double x = colWidth * m.Frame;
				double y = top + m.Line * 6;
				DrawKey(ptr, m, x, y);
			}
		}

		private void DrawKey(Xwt.Drawing.Context ctx, KeyTransient m, double x, double y)
		{
			DrawCounter++;
			//ctx.DrawImage(image, x, y);
			ctx.SetColor(m.XwtColor);
			ctx.Rectangle(x + 3, y + 1, 6, 6);
			//ctx.MoveTo(x + 3, y + 1);
			//ctx.LineTo(x + 3 + 6, y + 1 + 3);
			//ctx.LineTo(x + 3, y + 1 + 6);
			//ctx.MoveTo(x + 3, y + 3);
			ctx.Fill();
			if (m.Length > 0) {
				double x1 = x + m.Length * colWidth;
				//ctx.SetLineDash(0, new double[] { 5, 5 });
				//ctx.MoveTo(x + 3, y + 3);
				//ctx.LineTo(x1, y + 3);
				//ctx.Stroke();
				ctx.Rectangle(x + 3, y + 3, x1 - x, 2);
				ctx.Fill();
			}
		}
	}
}
