using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class NodesList : QWidget
	{
		QImage eyeIcon = Toolbox.LoadIcon("Timeline/Eye");
		QImage dotIcon = Toolbox.LoadIcon("Timeline/Dot");
		QImage nodeIcon = Toolbox.LoadIcon("Nodes/Scene");

		int ActiveRow { get { return The.Timeline.ActiveRow; } set { The.Timeline.ActiveRow = value; } }
		int RowHeight { get { return The.Timeline.RowHeight; } set { The.Timeline.RowHeight = value; } }

		public NodesList()
		{
			this.Paint += NodesList_Paint;
			this.GrabKeyboard();
			this.KeyPress += NodesList_KeyPress;
		}

		void NodesList_KeyPress(object sender, QEventArgs<QKeyEvent> e)
		{
			switch ((Qt.Key)e.Event.Key()) {
				case Qt.Key.Key_Down:
					ActiveRow++;
					Timeline.Instance.Dock.Update();
					//this.Update();
					break;
				case Qt.Key.Key_Up:
					ActiveRow--;
					Timeline.Instance.Dock.Update();
					//this.Update();
					break;
			}
		}

		void NodesList_Paint(object sender, QEventArgs<QPaintEvent> e)
		{
			var ptr = new QPainter(this);
			ptr.FillRect(Rect, GlobalColor.white);
			int width = Width;
			var nodes = The.Document.RootNode.Nodes;
			// Draw node separation lines and selection
			ptr.Pen = new QPen(GlobalColor.darkGray);
			for (int i = 0; i < nodes.Count; i++) {
				int y = i * RowHeight;
				ptr.DrawLine(0, y + RowHeight, width, y + RowHeight);
				if (i == ActiveRow) {
					ptr.FillRect(0, y + 1, width, RowHeight - 1, GlobalColor.lightGray);
				}
			}
			// Draw node names
			ptr.Pen = new QPen(GlobalColor.black);
			ptr.Font = new QFont("Tahoma", 9);
			int fontHeight = ptr.FontMetrics().Height() - 2;
			for (int i = 0; i < nodes.Count; i++) {
				int y = i * RowHeight;
				ptr.DrawText(35, (y + RowHeight) - (RowHeight - fontHeight) / 2, nodes[i].Id);
				ptr.DrawImage(10, y + (RowHeight - 16) / 2, nodeIcon);
				ptr.DrawImage(width - 16, y + (RowHeight - 16) / 2, dotIcon);
				ptr.DrawImage(width - 32, y + (RowHeight - 16) / 2, dotIcon);
			}
			ptr.Pen = new QPen(GlobalColor.red);
			ptr.End();
		}
	}
}
