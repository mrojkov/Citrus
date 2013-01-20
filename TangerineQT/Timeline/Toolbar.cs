using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine.Timeline
{
	public class Toolbar : QWidget
	{
		Document doc { get { return The.Document; } }
		CachedTextPainter textPainter = new CachedTextPainter();

		public Toolbar()
		{
			this.SetFixedHeight(doc.RowHeight);
			Paint += this_Paint;
		}

		void this_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			using (var ptr = new QPainter(this)) {
				var mem = System.GC.GetTotalMemory(true);
				//textPainter.Draw(ptr, 0, 0, (mem / 1024).ToString("Memory: 0kb"));
			}
		}
	}
}
