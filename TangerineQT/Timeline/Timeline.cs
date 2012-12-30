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
		public static int RowHeight { get { return The.Preferences.TimelineRowHeight; } }
		public static int ColWidth { get { return The.Preferences.TimelineColWidth; } }

		public QDockWidget DockWidget { get; private set; }

		public int ActiveRow;
		public int ToRow;
		public int LeftCol;

		public static Timeline Instance = new Timeline();

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

		private Timeline()
		{
			DockWidget = new QDockWidget("Timeline", MainWindow.Instance);
			DockWidget.ObjectName = "Timeline";
			DockWidget.Font = new QFont("Tahoma", 9);
			FontHeight = DockWidget.FontMetrics().Height();
			splitter1 = new QSplitter(Qt.Orientation.Vertical);
			splitter2 = new QSplitter(Qt.Orientation.Horizontal);
			splitter2.AddWidget(CreateToolbarAndNodeRoll());
			splitter2.AddWidget(CreateRulerAndGrid());
			splitter1.AddWidget(new Panview());
			splitter1.AddWidget(splitter2);
			DockWidget.Widget = splitter1;
			splitter1.Sizes = new List<int> { 30, 100 };
			splitter2.Sizes = new List<int> { 30, 100 };
			DockWidget.KeyPress += OnKeyPressed;
			Controller = new TimelineController();
			DockWidget.MousePress += Dock_MousePress;
		}

		void Dock_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
//			QWidget w = QApplication.FocusWidget();
//			if (w == null && !Dock.IsAncestorOf(w)) {
				DockWidget.SetFocus();
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
