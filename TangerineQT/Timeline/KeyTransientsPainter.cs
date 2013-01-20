using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine.Timeline
{
	public class KeyTransientsPainter
	{
		float colWidth;
		float top;

		public KeyTransientsPainter(float colWidth, float top)
		{
			this.colWidth = colWidth;
			this.top = top;
		}

		public void DrawTransients(List<KeyTransient> transients, QPainter ptr)
		{
			for (int i = 0; i < transients.Count; i++) {
				var m = transients[i];
				int x = (int)(colWidth * m.Frame);
				int y = (int)(top + m.Line * 6);
				DrawKey(ptr, m, x, y);
			}
		}

		private void DrawKey(QPainter ptr, KeyTransient m, int x, int y)
		{
			ptr.FillRect(x + 3, y + 1, 6, 6, m.QColor);
			if (m.Length > 0) {
				int x1 = (int)(x + m.Length * colWidth);
				ptr.FillRect(x + 3, y + 3, x1 - x, 2, m.QColor);
			}
		}
	}
}
