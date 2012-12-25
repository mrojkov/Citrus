using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public delegate void KeyboardHandler(Qt.Key key);

	public class Timeline
	{
		public QDockWidget Dock { get; private set; }

		public int RowHeight = 30;
		public int ColWidth = 10;
		public int ActiveRow;
		public int ToRow;
		public int LeftCol;

		public CachedTextPainter TextPainter = new CachedTextPainter();

		public static Timeline Instance;

		public TimelineController Controller;

		QSplitter splitter1;
		QSplitter splitter2;

		public int FontHeight { get; private set; }
		public NodeRoll NodeRoll;
		public KeyGrid KeyGrid;

		public event Lime.BareEventHandler ActiveRowChanged = () => {};
		public event KeyboardHandler KeyPressed = (key) => {};

		public void OnActiveRowChanged()
		{
			ActiveRowChanged();
		}

		public Timeline()
		{
			Instance = this;
			Dock = new QDockWidget("Timeline", MainWindow.Instance);
			Dock.ObjectName = "Timeline";
			Dock.SetAllowedAreas(Qt.DockWidgetArea.TopDockWidgetArea | Qt.DockWidgetArea.BottomDockWidgetArea);
			Dock.Font = new QFont("Tahoma", 9);
			FontHeight = Dock.FontMetrics().Height();
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
			Dock.KeyPress += OnKeyPressed;
			Controller = new TimelineController();
			Dock.MousePress += Dock_MousePress;
		}

		void Dock_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
//			QWidget w = QApplication.FocusWidget();
//			if (w == null && !Dock.IsAncestorOf(w)) {
				Dock.SetFocus();
//			}
		}

		void OnKeyPressed(object sender, QEventArgs<QKeyEvent> e)
		{
			QWidget w = QApplication.FocusWidget();
			if (w != null && w is QLineEdit) {
				return;
			}
			KeyPressed((Qt.Key)e.Event.Key());
		}

		private QWidget CreateRulerAndGrid()
		{
			var ctr = new QWidget();
			var layout = new QVBoxLayout(ctr);
			layout.Spacing = 0;
			layout.Margin = 0;
			layout.AddWidget(new KeyGridRuler());
			KeyGrid = new KeyGrid();
			layout.AddWidget(KeyGrid);
			return ctr;
		}

		private QWidget CreateToolbarAndNodeRoll()
		{
			var ctr = new QWidget();
			var layout = new QVBoxLayout(ctr);
			layout.Spacing = 0;
			layout.Margin = 0;
			layout.AddWidget(new TimelineToolbar());
			NodeRoll = new NodeRoll();
			layout.AddWidget(NodeRoll);
			return ctr;
		}
	}
}
