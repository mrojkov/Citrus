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
		public List<AbstractLine> Lines = new List<AbstractLine>();
		public LinesBuilder LinesBuilder = new LinesBuilder();

		public static int RowHeight { get { return The.Preferences.TimelineRowHeight; } }
		public static int ColWidth { get { return The.Preferences.TimelineColWidth; } }

		public QDockWidget DockWidget { get; private set; }

		public int TopLine;
		public int LeftCol;

		public static Timeline Instance = new Timeline();

		QSplitter splitter1;
		QSplitter splitter2;

		public int FontHeight { get; private set; }
		public NodeRoll NodeRoll;
		public KeyGrid KeyGrid;

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
			DockWidget.MousePress += Dock_MousePress;
			Document.Changed += Document_Changed;
			DockWidget.Wheel += DockWidget_Wheel;
		}

		void DockWidget_Wheel(object sender, QEventArgs<QWheelEvent> e)
		{
			TopLine += e.Event.Delta() > 0 ? -1 : 1;
			int m = Lines.Count - GetNumberOfVisibleLines();
			Lime.Mathf.Clamp(ref TopLine, 0, m);
			Refresh();
		}

		void Dock_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
//			QWidget w = QApplication.FocusWidget();
//			if (w == null && !Dock.IsAncestorOf(w)) {
				DockWidget.SetFocus();
//			}
		}

		private void Document_Changed()
		{
			Refresh();
		}

		public void Refresh()
		{
			var newLines = LinesBuilder.BuildLines(The.Document.Container);
			if (!Lines.AreEqual(newLines)) {
				NodeRoll.Hide();
				RefreshLines(newLines);
				NodeRoll.Show();
				KeyGrid.Update();
			}
			foreach (var line in Lines) {
				bool lineSelected = The.Document.SelectedLines.Contains(line.Index);
				line.RefreshPosition();
				line.SetSelected(lineSelected);
			}
			KeyGrid.Update();
		}

		private void RefreshLines(List<AbstractLine> newLines)
		{
			var document = The.Document;
			foreach (var line in Lines) {
				line.Detach();
			}
			Lines.Clear();
			int i = 0;
			foreach (var line in newLines) {
				line.Attach(i++);
				Lines.Add(line);
			}
		}

		public void EnsureLineVisible(int line)
		{
			if (line < TopLine) {
				TopLine = line;
			} else if (line > TopLine + GetNumberOfVisibleLines()) {
				TopLine = line - GetNumberOfVisibleLines();
			}
		}

		public int GetNumberOfVisibleLines()
		{
			int numVisibleRows = (NodeRoll.Height - RowHeight + 1) / RowHeight;
			return numVisibleRows;
		}

		//void OnKeyPressed(object sender, QEventArgs<QKeyEvent> e)
		//{
		//	QWidget w = QApplication.FocusWidget();
		//	if (w != null && w is QLineEdit) {
		//		return;
		//	}
		//	KeyPressed((Qt.Key)e.Event.Key());
		//}

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
