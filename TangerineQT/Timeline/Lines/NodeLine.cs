using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;
using System.Reflection;

namespace Tangerine
{
	/// <summary>
	/// Строка представляющая один нод на таймлайне
	/// </summary>
	public class NodeLine : AbstractLine
	{
		private Lime.Node node;
		private NodeSettings settings;

		public bool IsFolded
		{
			get { return settings.IsFolded; }
		}

		public NodeLine(Lime.Node node)
		{
			settings = The.Document.Settings.GetObjectSettings<NodeSettings>(node.Guid.ToString());
			this.node = node;
		}

		public override void CreateWidgets()
		{
			base.CreateWidgets();
			//var expanderIcon = CreateIconButton("Timeline/Collapsed");
			//expanderIcon.Clicked += expanderIcon_Clicked;
			//layout.AddWidget(expanderIcon);

			var nodeIcon = CreateImageWidget("Nodes/Scene");
			//nodeIcon.Clicked += nodeIcon_Clicked;
			layout.AddWidget(nodeIcon);

			var label = new QLabel(node.Id);
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

		public override void PaintContent(QPainter ptr, int width)
		{
			int numCols = width / ColWidth + 1;
			var c = new KeyTransientCollector();
			var tp = new KeyTransientsPainter(ColWidth, Top);
			var transients = c.GetTransients(node);
			tp.DrawTransients(transients, 0, numCols, ptr);
		}
	}
}
