using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class NodeRoll : QWidget
	{
		Document doc { get { return The.Document; } }

		public NodeRoll()
		{
			AutoFillBackground = true;
			Palette = new QPalette(GlobalColor.white);
			MousePress += NodeRoll_MousePress;
		}

		public override QSize SizeHint()
		{
			return new QSize(0, The.Preferences.TimelineDefaultHeight);
		}

		void NodeRoll_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
			int row = e.Event.Y() / doc.RowHeight + doc.TopRow;
			if (row >= 0 && row < doc.Rows.Count) {
				The.Document.History.Add(new Commands.SelectRows(row));
				The.Document.History.Commit("Select Line");
			}
		}
	}
}
