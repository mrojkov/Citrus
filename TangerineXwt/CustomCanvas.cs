using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public delegate void CanvasDrawDelegate(Xwt.Drawing.Context ctx, Xwt.Rectangle dirtyRect);

	public class CustomCanvas : Xwt.Canvas
	{
		public event CanvasDrawDelegate Drawn;

		protected override void OnDraw(Xwt.Drawing.Context ctx, Xwt.Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);
			if (Drawn != null) {
				Drawn(ctx, dirtyRect);
			}
		}
	}
}
