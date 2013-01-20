using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine.Timeline
{
	public class Roll : QWidget
	{
		Document doc { get { return The.Document; } }

		public Roll()
		{
			AutoFillBackground = true;
			Palette = new QPalette(GlobalColor.white);
			MousePress += this_MousePress;
		}

		public override QSize SizeHint()
		{
			return new QSize(0, The.Preferences.TimelineDefaultHeight);
		}

		void this_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
			int row = e.Event.Y() / doc.RowHeight + doc.TopRow;
			if (row >= 0 && row < doc.Rows.Count) {
				The.Document.History.Add(new Commands.SelectRows(row));
				The.Document.History.Commit("Select Line");
			}
		}
	}
}
