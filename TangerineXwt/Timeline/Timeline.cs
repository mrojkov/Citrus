using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace Tangerine.Timeline
{
	public class Timeline
	{
		Document doc { get { return The.Document; } }

		public static Timeline Instance = new Timeline();

		private List<Row> rows = new List<Row>();

		public Xwt.Window Window { get; private set; }

		Xwt.Paned panviewSplitter;
		Xwt.Paned rollAndGridSplitter;

		public int FontHeight { get; private set; }

		public Xwt.Box Panel;	
		public Roll Roll;
		public Grid Grid;
		public Panview Panview;
		public Ruler Ruler;
		public Toolbar Toolbar;

		private List<XwtPlus.Command> actions = new List<XwtPlus.Command>();

		private Timeline()
		{
			Window = new Xwt.Window() { Title = "Timeline", Size = new Xwt.Size(1000, 300) };
			Window.Padding = 0;
			Window.Location = new Xwt.Point(0, 0);
			rollAndGridSplitter = new Xwt.HPaned();
			Grid = new Grid();
			Roll = new Roll();
			Panview = new Panview();
			Ruler = new Ruler();
			Toolbar = new Toolbar();
			Document.Changed += Document_Changed;

			var rulerAndGridBox = new Xwt.VBox();
			rulerAndGridBox.PackStart(Ruler.Canvas);
			rulerAndGridBox.PackStart(Grid.Canvas, Xwt.BoxMode.Expand);

			var toolbarAndRowBox = new Xwt.VBox();
			toolbarAndRowBox.PackStart(Toolbar.Canvas);
			toolbarAndRowBox.PackStart(Roll.Canvas, Xwt.BoxMode.Expand);

			rollAndGridSplitter.Panel1.Content = toolbarAndRowBox;
			rollAndGridSplitter.Panel2.Content = rulerAndGridBox;

			panviewSplitter = new Xwt.VPaned();
			panviewSplitter.Panel1.Content = Panview.Canvas;
			panviewSplitter.Panel1.Shrink = true;
			panviewSplitter.Panel2.Content = rollAndGridSplitter;

			Panel = new Xwt.HBox();
			Panel.PackStart(panviewSplitter, Xwt.BoxMode.Expand);
			Panel.CanGetFocus = true;
			Panel.ButtonPressed += Box_ButtonPressed;
		
			// Имеется баг в Xwt.WPF. Не устанавливаются позиции сплиттеров. 
			// Используем workaround с изменением MinWidth, MinHeight
			panviewSplitter.Position = 50;
			rollAndGridSplitter.Position = 100;
			Panview.Canvas.MinHeight = The.Preferences.TimelineDefaultPanviewHeight;
			Roll.Canvas.MinWidth = The.Preferences.TimelineDefaultRollWidth;

			Window.Content = Panel;
			Window.Show();
			CreateActions();
		}

		void Box_ButtonPressed(object sender, Xwt.ButtonEventArgs e)
		{
			if (e.MultiplePress == 1) {
				Panel.SetFocus();
			}
		}

		//XwtPlus.Command command;
		//XwtPlus.Command command2;

		//private void CreateMenu()
		//{
		//	var menu = new Xwt.Menu();
		//	Window.MainMenu = menu;
		//	var file = new XwtPlus.MenuItem() { Label = "FILE" };
		//	menu.Items.Add(file);
		//	file.SubMenu = new Xwt.Menu();
		//	var test = new XwtPlus.MenuItem();
		//	file.SubMenu.Items.Add(test);
		//	var command = new XwtPlus.Command("XXX", "Ctrl+F");
		//	command.Bind(test);
		//	command.Triggered += command_Triggered;

		//	var test2 = new XwtPlus.MenuItem();
		//	file.SubMenu.Items.Add(test2);
		//	var command2 = new XwtPlus.Command("YYY", "Ctrl+Alt+W");
		//	command2.Bind(test2);
		//	command2.Triggered += command2_Triggered;
		//	//command.KeySequence = "Right";
		//	//up.Command = command;
		//}

		//void command_Triggered()
		//{
		//	Console.WriteLine("1");
		//}

		//void command2_Triggered()
		//{
		//	Console.WriteLine("2");
		//}

		private void CreateActions()
		{
			AddAction(new ChoosePrevNode());
			AddAction(new ChooseNextNode());
			AddAction(new EnterContainer());
			AddAction(new ExitContainer());
			AddAction(new MoveCursorRight());
			AddAction(new MoveCursorLeft());
			AddAction(new MoveCursorRightFast());
			AddAction(new MoveCursorLeftFast());
		}

		private void AddAction(XwtPlus.Command command)
		{
			actions.Add(command);
			command.Context = Panel;
		}

		public void EnableActions(bool enable)
		{
			foreach (var action in actions) {
				action.Sensitive = enable;
			}
		}

		//void DockWidget_Wheel(object sender, QEventArgs<QWheelEvent> e)
		//{
		//	doc.TopRow += e.Event.Delta() > 0 ? -1 : 1;
		//	int m = Math.Max(0, rows.Count - Toolbox.CalcNumberOfVisibleRows());
		//	doc.TopRow = Lime.Mathf.Clamp(doc.TopRow, 0, m);
		//	Refresh();
		//}

		//void Dock_MousePress(object sender, QEventArgs<QMouseEvent> e)
		//{
		//	DockWidget.SetFocus();
		//}

		private void Document_Changed()
		{
			Refresh();
		}

		public void Refresh()
		{
			var newRows = The.Document.Rows;
			if (!rows.AreEqual(newRows)) {
				Roll.Canvas.Hide();
				RefreshRows(newRows);
				Roll.Canvas.Show();
			}
			foreach (var row in rows) {
				row.View.Refresh();
				bool rowSelected = The.Document.SelectedRows.Contains(row.Index);
				row.View.SetSelected(rowSelected);
			}
			Roll.Canvas.QueueDraw();
			Grid.Canvas.QueueDraw();
			Panview.Canvas.QueueDraw();
			Ruler.Canvas.QueueDraw();
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
			row = row.Clamp(0, doc.Rows.Count);
			if (row < doc.TopRow) {
				doc.TopRow = row;
			} else if (row >= doc.TopRow + Toolbox.CalcNumberOfVisibleRows()) {
				doc.TopRow = row - Toolbox.CalcNumberOfVisibleRows() + 1;
			}
		}

		public void EnsureColumnVisible(int column)
		{
			column = Math.Max(0, column);
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

		//private QWidget CreateRulerAndGrid()
		//{
		//	var ctr = new QWidget();
		//	var layout = new QVBoxLayout(ctr);
		//	layout.Spacing = 0;
		//	layout.Margin = 0;
		//	Ruler = new Ruler();
		//	layout.AddWidget(Ruler);
		//	Grid = new Grid();
		//	layout.AddWidget(Grid);
		//	return ctr;
		//}

		//private QWidget CreateToolbarAndNodeRoll()
		//{
		//	var ctr = new QWidget();
		//	var layout = new QVBoxLayout(ctr);
		//	layout.Spacing = 0;
		//	layout.Margin = 0;
		//	layout.AddWidget(new Toolbar());
		//	Roll = new Roll();
		//	layout.AddWidget(Roll);
		//	return ctr;
		//}
	}
}
