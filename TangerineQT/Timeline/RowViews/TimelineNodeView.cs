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
		private QLabel nodeName;
		private InplaceTextEditor inplaceEditor;

		public new TimelineNodeRow row { get { return base.row as TimelineNodeRow; } }

		public TimelineNodeView(TimelineRow row)
			: base(row)
		{
			//var expanderIcon = CreateIconButton("Timeline/Collapsed");
			//expanderIcon.Clicked += expanderIcon_Clicked;
			//layout.AddWidget(expanderIcon);

			//var nodeIcon = CreateIconButton("Nodes/Scene");
			var nodeIcon = CreateImageWidget("Nodes/Scene");
			//nodeIcon.Clicked += nodeIcon_Clicked;
			layout.AddWidget(nodeIcon, 1);

			nodeName = new QLabel(this.row.Node.Id);
			nodeName.MouseDoubleClick += nodeName_MouseDoubleClick;
			layout.AddWidget(nodeName, 10);

			//var spacing = new QLabel();
			//layout.AddWidget(spacing, 10);

			var bt = CreateImageWidget("Timeline/Dot");
			bt.MousePress += bt_MousePress;
			layout.AddWidget(bt);

			var bt2 = CreateImageWidget("Timeline/Dot");
			layout.AddWidget(bt2, 0);
		}

		void nodeName_MouseDoubleClick(object sender, QEventArgs<QMouseEvent> e)
		{
			NodeRoll.MouseDoubleClickConsumed = true;
			inplaceEditor = new InplaceTextEditor(nodeName);
			inplaceEditor.Finished += (text) => {
				this.row.Node.Id = text;
			};
		}

		void bt_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
			NodeRoll.MousePressConsumed = true;
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
			int numCols = width / doc.ColumnWidth + 1;
			var c = new KeyTransientCollector();
			var tp = new KeyTransientsPainter(doc.ColumnWidth, top);
			var transients = c.GetTransients(row.Node);
			tp.DrawTransients(transients, ptr);
		}

		private void DrawKey(QPainter ptr, KeyTransient m, int x, int y)
		{
			ptr.FillRect(x, y, 2, 2, m.QColor);
		}
	}
}
