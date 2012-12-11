using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class Timeline
	{
		public QDockWidget Dock { get; private set; }

		public int RowHeight = 30;
		public int ColWidth = 10;
		public int ActiveRow;

		public static Timeline Instance;

		QSplitter splitter1;
		QSplitter splitter2;

		public Timeline()
		{
			Instance = this;
			Dock = new QDockWidget("Timeline", MainWindow.Instance);
			Dock.ObjectName = "Timeline";
			Dock.SetAllowedAreas(Qt.DockWidgetArea.TopDockWidgetArea | Qt.DockWidgetArea.BottomDockWidgetArea);
			splitter1 = new QSplitter(Qt.Orientation.Vertical);
			splitter2 = new QSplitter(Qt.Orientation.Horizontal);
			splitter2.AddWidget(new NodesList());
			splitter2.AddWidget(new Keygrid());
			splitter1.AddWidget(new Panview());
			splitter1.AddWidget(splitter2);
			Dock.Widget = splitter1;
			Dock.MinimumHeight = 30;
			splitter1.Sizes = new List<int> { 25, 100 };
			splitter2.Sizes = new List<int> { 25, 100 };
		}
	}
}
