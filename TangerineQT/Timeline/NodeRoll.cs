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
			int line = e.Event.Y() / Timeline.RowHeight + The.Timeline.TopLine;
			if (line >= 0 && line < The.Timeline.Lines.Count) {
				The.Document.History.Add(new Commands.SelectLines(line));
				The.Document.History.Commit("Select Line");
			}
		}
	}
}
