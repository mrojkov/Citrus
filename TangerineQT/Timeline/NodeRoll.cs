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
			int row = e.Event.Y() / Timeline.RowHeight;
			if (row >= 0 && row < The.Timeline.Controller.Items.Count) {
				The.Timeline.ActiveRow = row;
				The.Timeline.OnActiveRowChanged();
			}
		}
	}
}
