using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public class DocumentsArea
	{
		public Xwt.Box Notebook;

		public DocumentsArea()
		{
			Notebook = new Xwt.HBox();// Notebook();
			//Notebook.BackgroundColor = Colors.ToolPanelBackground;
			//Notebook.Add(new Xwt.Canvas() { BackgroundColor = Colors.ActiveBackground }, "Document1");
			//Notebook.Add(new Xwt.Canvas() { BackgroundColor = Colors.ActiveBackground }, "Document2");
		}
	}

	class MainWindow
	{
		public Xwt.Window Window { get; private set; }
		public Xwt.HBox DockingArea;
		public DocumentsArea DocumentsArea { get; private set; }
		public DockManager DockSiteManager { get; private set; }
		public static MainWindow Instance;

		public MainWindow()
		{
			Instance = this;
			Window = new Xwt.Window() { Width = 800, Height = 600 };
			Window.Padding = new Xwt.WidgetSpacing();
			DockingArea = new Xwt.HBox();
			DocumentsArea = new DocumentsArea();
			DockingArea.PackStart(DocumentsArea.Notebook, Xwt.BoxMode.Expand);
			Window.Content = DockingArea;
			DockingArea.MouseMoved += (a, e) => { Console.WriteLine("Docking area move"); };
			Window.Show();

			DockSiteManager = new DockManager(DockingArea) { CentralWidget = DocumentsArea.Notebook	 };

			var panel2 = new DockPanel(DockSiteManager) { Title = "ActorInspector" };
			var panel3 = new DockPanel(DockSiteManager) { Title = "ActorList" };
			//panel2.Dock(DockZone.Right);
			//panel3.Dock(DockZone.Bottom);
			CreatePanelSampleContent(panel2);
			CreatePanelSampleContent(panel3);
			CreateMenu();
		}

		private void CreatePanelSampleContent(DockPanel panel)
		{
			//Content.MinWidth = 30;
			//Content.MinHeight = 200;
			//Content.NaturalWidth = 300;
			var box1 = new Xwt.VBox();
			panel.Content = box1;
			box1.Margin = new Xwt.WidgetSpacing(4, 4, 4, 4);
			box1.PackStart(new Xwt.RichTextView(), Xwt.BoxMode.Fill);
			box1.PackStart(new Xwt.Button("Test"), Xwt.BoxMode.Fill);
			box1.PackStart(new Xwt.ComboBox(), Xwt.BoxMode.Fill);
		}

		private void CreateMenu()
		{
			var menu = new Xwt.Menu();
			Window.MainMenu = menu;
			//var box = new Xwt.VBox();
			//box.CanGetFocus = true;
			//box.SetFocus();
			//Window.Content = box;
			//box.PackStart(new Xwt.Button("XXX"));
			//var combo = new Xwt.ComboBox();
			//combo.Items.Add("XXXX");
			//combo.Items.Add("YYY");
			//combo.Items.Add("ZZZZ");
			//box.PackStart(combo);

			var file = new XwtPlus.MenuItem() { Label = "FILE" };
			menu.Items.Add(file);
			file.SubMenu = new Xwt.Menu();
			var test = new XwtPlus.MenuItem();
			file.SubMenu.Items.Add(test);
			file.SubMenu.Items.Add(new Xwt.SeparatorMenuItem());
			var quit = new XwtPlus.MenuItem() { Label = "Quit" };
			file.SubMenu.Items.Add(quit);
			var command = new XwtPlus.Command("XXX", "Ctrl+A");
			command.Visible = false;
			command.Bind(test);
			command.Triggered += command_Triggered;

			var test2 = new XwtPlus.MenuItem();
			file.SubMenu.Items.Add(test2);
			var command2 = new XwtPlus.Command("YYY", "Ctrl+R");
			command2.Bind(test2);
			command2.Triggered += command2_Triggered;
			//command.KeySequence = "Right";
			//up.Command = command;
		}

		void command_Triggered()
		{
			Console.WriteLine("3");
		}

		void command2_Triggered()
		{
			Console.WriteLine("4");
		}
	}
}
