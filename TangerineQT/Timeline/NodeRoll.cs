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

		public static bool MousePressConsumed;
		public static bool MouseDoubleClickConsumed;

		public NodeRoll()
		{
			AutoFillBackground = true;
			Palette = new QPalette(GlobalColor.white);
			MousePress += NodeRoll_MousePress;
			MouseDoubleClick += NodeRoll_MouseDoubleClick;
		}

		void NodeRoll_MouseDoubleClick(object sender, QEventArgs<QMouseEvent> e)
		{
			if (!MouseDoubleClickConsumed) {
				var lines = The.Document.SelectedRows;
				if (lines.Count > 0) {
					var container = The.Document.Container.Nodes[lines[0]];
					The.Document.History.Add(new Commands.ChangeContainer(container));
					The.Document.History.Commit("Change container");
				}
			}
			MouseDoubleClickConsumed = false;
		}

		public override QSize SizeHint()
		{
			return new QSize(0, The.Preferences.TimelineDefaultHeight);
		}

		void NodeRoll_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
			if (!MousePressConsumed) {
				int row = e.Event.Y() / doc.RowHeight + doc.TopRow;
				if (row >= 0 && row < doc.Rows.Count) {
					The.Document.History.Add(new Commands.SelectRows(row));
					The.Document.History.Commit("Select Line");
				}
			}
			MousePressConsumed = false;
		}
	}
}
