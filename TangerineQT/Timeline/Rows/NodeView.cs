using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;
using System.Reflection;

namespace Tangerine.Timeline
{
	public class NodeView : RowView
	{
		private QLabel nodeName;
		private InplaceTextEditor inplaceEditor;

		public new NodeRow row { get { return base.row as NodeRow; } }

		public NodeView(Row row)
			: base(row)
		{
			//var expanderIcon = CreateIconButton("Timeline/Collapsed");
			//expanderIcon.Clicked += expanderIcon_Clicked;
			//layout.AddWidget(expanderIcon);

			nodeName = new QLabel(this.row.Node.Id);
			//var nodeIcon = CreateIconButton("Nodes/Scene");
			var nodeIcon = CreateImageWidget("Nodes/Scene");
			//nodeIcon.Clicked += nodeIcon_Clicked;
			layout.AddWidget(nodeIcon, 1);

			nodeName.MouseDoubleClick += nodeName_MouseDoubleClick;
			layout.AddWidget(nodeName, 10);

			//var spacing = new QLabel();
			//layout.AddWidget(spacing, 10);

			var bt = CreateImageWidget("Timeline/Dot");
			layout.AddWidget(bt);

			var bt2 = CreateImageWidget("Timeline/Dot");
			layout.AddWidget(bt2, 0);
		}

		void nodeName_MouseDoubleClick(object sender, QEventArgs<QMouseEvent> e)
		{
			var label = (QLabel)sender;
			var metrics = new QFontMetrics(label.Font);
			int textWidth = metrics.Width(label.Text);
			bool hitLabel = e.Event.X() < textWidth;
			if (hitLabel) {
				RenameNode();
			} else {
				EnterIntoNode();
			}
		}

		public override void Refresh()
		{
			nodeName.Text = row.Node.Id;
		}

		private static void EnterIntoNode()
		{
			var lines = The.Document.SelectedRows;
			if (lines.Count > 0) {
				var container = The.Document.Container.Nodes[lines[0]];
				The.Document.History.Add(new Commands.ChangeContainer(container));
				The.Document.History.Commit("Change container");
			}
		}

		private void RenameNode()
		{
			The.Timeline.EnableActions(false);
			inplaceEditor = new InplaceTextEditor(nodeName);
			inplaceEditor.Finished += (text) => {
				The.Timeline.EnableActions(true);
				//The.Timeline.DockWidget.SetDisabled(false);
				The.Document.History.Add(new Commands.ChangeNodeProperty(row.Node, "Id", text));
				The.Document.History.Commit("Rename node");
			};	
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
