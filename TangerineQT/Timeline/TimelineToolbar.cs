using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class TimelineToolbar : QToolBar
	{
		public TimelineToolbar()
		{
			this.SetFixedHeight(30);//The.Timeline.RowHeight);
			Paint += KeyGridRuler_Paint;
		}

		void KeyGridRuler_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			using (var ptr = new QPainter(this)) {
				var mem = System.GC.GetTotalMemory(true);
				ptr.DrawText(0, Height - 10, (mem / 1024).ToString("Memory: 0kb"));
			}
		}
	}
}
