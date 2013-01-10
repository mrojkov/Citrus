using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;
using System.Reflection;

namespace Tangerine
{
	public class TimelineNodeView : TimelineRowView
	{
		public new TimelineNodeRow row { get { return base.row as TimelineNodeRow; } }

		public TimelineNodeView(TimelineRow row)
			: base(row)
		{
			//var expanderIcon = CreateIconButton("Timeline/Collapsed");
			//expanderIcon.Clicked += expanderIcon_Clicked;
			//layout.AddWidget(expanderIcon);

			var nodeIcon = CreateImageWidget("Nodes/Scene");
			//nodeIcon.Clicked += nodeIcon_Clicked;
			layout.AddWidget(nodeIcon);

			var label = new QLabel(this.row.Node.Id);
			layout.AddWidget(label, 10);

			var bt = CreateImageWidget("Timeline/Dot");
			layout.AddWidget(bt);

			bt = CreateImageWidget("Timeline/Dot");
			layout.AddWidget(bt, 0);
		}

		//void expanderIcon_MousePress(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	settings.IsFolded = !settings.IsFolded;
		//	The.Timeline.RefreshLines();
		//}

		//public override void HandleKey(Qt.Key key)
		//{
		//	if (key == Key.Key_Return) {
		//		settings.IsFolded = !settings.IsFolded;
		//		The.Timeline.RefreshLines();
		//	}
		//}

		//[Q_SLOT]
		//void nodeIcon_Clicked()
		//{
		//	settings.IsFolded = !settings.IsFolded;
		//	The.Timeline.Controller.Rebuild();
		//}

		public override void PaintContent(QPainter ptr, int top, int width)
		{
			int numCols = width / ColWidth + 1;
			var c = new KeyTransientCollector();
			var tp = new KeyTransientsPainter(ColWidth, top);
			var transients = c.GetTransients(row.Node);
			tp.DrawTransients(transients, 0, numCols, ptr);
		}

		private void DrawKey(QPainter ptr, KeyTransient m, int x, int y)
		{
			ptr.FillRect(x, y, 2, 2, m.QColor);
		}
	}
}
