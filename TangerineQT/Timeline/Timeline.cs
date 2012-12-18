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
		public int ToRow;
		public int LeftCol;

		public static Timeline Instance;

		QSplitter splitter1;
		QSplitter splitter2;

		public Timeline()
		{
			Instance = this;
			Dock = new QDockWidget("Timeline", MainWindow.Instance);
			Dock.ObjectName = "Timeline";
			Dock.SetAllowedAreas(Qt.DockWidgetArea.TopDockWidgetArea | Qt.DockWidgetArea.BottomDockWidgetArea);
			Dock.Font = new QFont("Tahoma", 9);
			splitter1 = new QSplitter(Qt.Orientation.Vertical);
			splitter2 = new QSplitter(Qt.Orientation.Horizontal);
			splitter2.AddWidget(CreateToolbarAndNodeRoll());
			splitter2.AddWidget(CreateRulerAndGrid());
			splitter1.AddWidget(new Panview());
			splitter1.AddWidget(splitter2);
			Dock.Widget = splitter1;
			Dock.MinimumHeight = 30;
			splitter1.Sizes = new List<int> { 10, 100 };
			splitter2.Sizes = new List<int> { 25, 100 };
			Dock.KeyPress += OnKeyPress;
		}

		public void GrabKeyboard()
		{
			Dock.GrabKeyboard();
		}

		void OnKeyPress(object sender, QEventArgs<QKeyEvent> e)
		{
			switch ((Qt.Key)e.Event.Key()) {
				case Qt.Key.Key_Down:
					ActiveRow++;
					Dock.Update();
					//this.Update();
					break;
				case Qt.Key.Key_Up:
					ActiveRow--;
					Dock.Update();
					//this.Update();
					break;
			}
		}

		private QWidget CreateRulerAndGrid()
		{
			var ctr = new QWidget();
			var layout = new QVBoxLayout(ctr);
			layout.Spacing = 0;
			layout.Margin = 0;
			layout.AddWidget(new KeyGridRuler());
			layout.AddWidget(new KeyGrid());
			return ctr;
		}

		private QWidget CreateToolbarAndNodeRoll()
		{
			var ctr = new QWidget();
			var layout = new QVBoxLayout(ctr);
			layout.Spacing = 0;
			layout.Margin = 0;
			layout.AddWidget(new TimelineToolbar());
			layout.AddWidget(new NodeRoll());
			return ctr;
		}
	}
}
