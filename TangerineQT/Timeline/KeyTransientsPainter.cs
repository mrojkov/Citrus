using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class KeyTransientsPainter
	{
		int colWidth;
		int top;

		public KeyTransientsPainter(int colWidth, int top)
		{
			this.colWidth = colWidth;
			this.top = top;
		}

		public void DrawTransients(List<KeyTransient> transients, int startFrame, int endFrame, QPainter ptr)
		{
			for (int i = 0; i < transients.Count; i++) {
				var m = transients[i];
				if (m.Frame >= startFrame && m.Frame < endFrame) {
					int x = colWidth * (m.Frame - startFrame) + 4;
					int y = top + m.Line * 6 + 4;
					DrawKey(ptr, m, x, y);
				}
			}
		}

		private void DrawKey(QPainter ptr, KeyTransient m, int x, int y)
		{
			ptr.FillRect(x - 3, y - 3, 6, 6, m.QColor);
			if (m.Length > 0) {
				int x1 = x + m.Length * colWidth;
				ptr.FillRect(x, y - 1, x1 - x, 2, m.QColor);
			}
		}
	}
}
