using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine.Timeline
{
	public class Timeline
	{
		Document doc { get { return The.Document; } }

		public static Timeline Instance = new Timeline();

		private List<Row> rows = new List<Row>();

		public QDockWidget DockWidget { get; private set; }

		QSplitter splitter1;
		QSplitter splitter2;

		public int FontHeight { get; private set; }
		public Roll Roll;
		public Grid Grid;
		public Panview Panview;
		public TimlineRuler Ruler;

		private List<Action> actions = new List<Action>();

		private Timeline()
		{
			DockWidget = new QDockWidget("Timeline", The.DefaultQtParent);
			DockWidget.ObjectName = "Timeline";
			DockWidget.Font = new QFont("Tahoma", 9);
			FontHeight = DockWidget.FontMetrics().Height();
			splitter1 = new QSplitter(Qt.Orientation.Vertical);
			splitter2 = new QSplitter(Qt.Orientation.Horizontal);
			splitter2.AddWidget(CreateToolbarAndNodeRoll());
			splitter2.AddWidget(CreateRulerAndGrid());
			Panview = new Panview();
			splitter1.AddWidget(Panview);
			splitter1.AddWidget(splitter2);
			DockWidget.Widget = splitter1;
			splitter1.Sizes = new List<int> { 30, 100 };
			splitter2.Sizes = new List<int> { 30, 100 };
			DockWidget.MousePress += Dock_MousePress;
			Document.Changed += Document_Changed;
			DockWidget.Wheel += DockWidget_Wheel;
			CreateActions();
		}

		private void CreateActions()
		{
			actions.Add(new ChoosePrevNode(DockWidget));
			actions.Add(new ChooseNextNode(DockWidget));
			actions.Add(new EnterContainer(DockWidget));
			actions.Add(new ExitContainer(DockWidget));
			actions.Add(new MoveCursorRight(DockWidget));
			actions.Add(new MoveCursorLeft(DockWidget));
			actions.Add(new MoveCursorRightFast(DockWidget));
			actions.Add(new MoveCursorLeftFast(DockWidget));
		}

		public void EnableActions(bool enable)
		{
			foreach (var action in actions) {
				action.Enabled = enable;
			}
		}

		void DockWidget_Wheel(object sender, QEventArgs<QWheelEvent> e)
		{
			doc.TopRow += e.Event.Delta() > 0 ? -1 : 1;
			int m = Math.Max(0, rows.Count - Toolbox.CalcNumberOfVisibleRows());
			doc.TopRow = Lime.Mathf.Clamp(doc.TopRow, 0, m);
			Refresh();
		}

		void Dock_MousePress(object sender, QEventArgs<QMouseEvent> e)
		{
			DockWidget.SetFocus();
		}

		private void Document_Changed()
		{
			Refresh();
		}

		public void Refresh()
		{
			var newRows = The.Document.Rows;
			if (!rows.AreEqual(newRows)) {
				Roll.Hide();
				RefreshRows(newRows);
				Roll.Show();
			}
			foreach (var row in rows) {
				row.View.Refresh();
				bool rowSelected = The.Document.SelectedRows.Contains(row.Index);
				row.View.SetPosition((row.Index - doc.TopRow) * doc.RowHeight);
				row.View.SetSelected(rowSelected);
			}
			Grid.Update();
			Ruler.Update();
			Panview.Update();
		}

		private void RefreshRows(List<Row> newRows)
		{
			foreach (var row in rows) {
				row.View.Detach();
			}
			rows.Clear();
			foreach (var row in newRows) {
				row.View.Attach();
				rows.Add(row);
			}
		}

		public void EnsureRowVisible(int row)
		{
			if (row < doc.TopRow) {
				doc.TopRow = row;
			} else if (row >= doc.TopRow + Toolbox.CalcNumberOfVisibleRows()) {
				doc.TopRow = row - Toolbox.CalcNumberOfVisibleRows() + 1;
			}
		}

		public void EnsureColumnVisible(int column)
		{
			if (column < doc.LeftColumn) {
				doc.LeftColumn = column;
			} else if (column >= doc.LeftColumn + Toolbox.CalcNumberOfVisibleColumns()) {
				doc.LeftColumn = column - Toolbox.CalcNumberOfVisibleColumns() + 1;
			}
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
			Ruler = new TimlineRuler();
			layout.AddWidget(Ruler);
			Grid = new Grid();
			layout.AddWidget(Grid);
			return ctr;
		}

		private QWidget CreateToolbarAndNodeRoll()
		{
			var ctr = new QWidget();
			var layout = new QVBoxLayout(ctr);
			layout.Spacing = 0;
			layout.Margin = 0;
			layout.AddWidget(new Toolbar());
			Roll = new Roll();
			layout.AddWidget(Roll);
			return ctr;
		}
	}
}
