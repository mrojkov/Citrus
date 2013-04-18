using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public class DockPanelLayout
	{
		public Xwt.Size DockedSize;
		public Xwt.Size FloatingSize = new Xwt.Size(400, 300);
		public bool Docked;
		public DockZone DockZone;
	}

	public interface IDockPanel
	{
		Xwt.Widget Widget { get; }
	}

	public class DockPanel : IDockPanel
	{
		private DockManager dockSiteManager;
		private Xwt.HBox windowContent;
		public Xwt.Window Window { get; private set; }
		public Xwt.VBox Content { get; private set; }
		public Xwt.Widget Widget { get { return headerAndContentBox; } }

		public DockPanelLayout Layout { get; private set; }

		public Xwt.Label TitleLabel { get; private set; }
		Xwt.VBox headerAndContentBox;

		public string Title
		{
			get { return TitleLabel.Text; }
			set { TitleLabel.Text = value; }
		}

		public bool Docked
		{
			get { return dockSiteManager.Panels.Contains(this); }
		}

		public void Undock()
		{
			if (!Docked) {
				throw new InvalidOperationException("Panel is not docked");
			}
			dockSiteManager.RemoveDockPanel(this);
			windowContent.PackStart(Widget, Xwt.BoxMode.Expand);
			Window.Size = Layout.FloatingSize;
			Window.Show();
			Window.TransientFor = MainWindow.Instance.Window;
		}

		bool holdOnTitle;

		//void title_ButtonPressed(object sender, Xwt.ButtonEventArgs e)
		//{
		//	if (e.Button == Xwt.PointerButton.Left) {
		//		holdOnTitle = true;
		//	}
		//}

		//void title_MouseMoved(object sender, Xwt.MouseMovedEventArgs e)
		//{
		//	Console.WriteLine("X" + e.X);
		//	if (holdOnTitle) {
		//		holdOnTitle = false;
		//		dockSiteManager.DragManager.StartDrag(this);

			
		//		//Undock();
		//		//Window.Location = new Xwt.Point(0, 10000);
		//		//Window.Y += 10000;

		//		//dockSiteManager.RemoveDockPanel(this);
				
		//		//var p = new Xwt.Point {
		//		//	X = title.ScreenBounds.X + e.X - 20,
		//		//	Y = title.ScreenBounds.Y + e.Y - 10
		//		//};
		//		////title.MouseMoved -= title_MouseMoved;
		//		//// Special hack, so move events now come to dragWindow
		//		//title.Visible = false;
		//		//title.Visible = true;
		//		//new DragWindow(p, Title);
		//	}
		//}

		//void title_ButtonReleased(object sender, Xwt.ButtonEventArgs e)
		//{
		//	if (e.Button == Xwt.PointerButton.Left) {
		//		holdOnTitle = false;
		//	}
		//}


		//bool dragging;

		//void Window_BoundsChanged(object sender, EventArgs e)
		//{
		//	dragging = true;
		//	bool buttonReleased = !XwtPlus.Utils.Instance.GetPointerButtonState(Xwt.PointerButton.Left);
		//	if (buttonReleased) {
		//		var ptr = XwtPlus.Utils.Instance.GetPointerPosition(Widget);
		//		DockZone zone;
		//		if (dockSiteManager.TryGetZoneForPointer(ptr, out zone)) {
		//			Dock(zone);
		//		}
		//	}
		//}

		public void Dock(DockZone zone)
		{
			if (Docked) {
				throw new InvalidOperationException("Panel has already docked");
			}
			Window.Hide();
			windowContent.Clear();
			dockSiteManager.AddDockPanel(this, zone);
			//TitleLabel.ButtonReleased += title_ButtonReleased;
			//TitleLabel.ButtonPressed += title_ButtonPressed;
			//TitleLabel.MouseMoved += title_MouseMoved;
		}

		public DockPanel(DockManager dockSiteManager, DockPanelLayout layout)
		{
			this.Layout = layout;
			this.dockSiteManager = dockSiteManager;
			CreateWindow();
			CreateContent();
			TitleLabel = new Xwt.Label("DockPanel");
			TitleLabel.MarginLeft = 4;
			TitleLabel.MarginBottom = 2;
			TitleLabel.MarginTop = 2;
			headerAndContentBox = new Xwt.VBox();
			headerAndContentBox.PackStart(TitleLabel);
			headerAndContentBox.PackStart(Content, Xwt.BoxMode.Expand);
			headerAndContentBox.BackgroundColor = Colors.ToolPanelBackground;
		}

		private void CreateWindow()
		{
			Window = new Xwt.Window() { ShowInTaskbar = false };
			windowContent = new Xwt.HBox();
			Window.Content = windowContent;
		}

		private void CreateContent()
		{
			Content = new Xwt.VBox();
			Content.Margin = new Xwt.WidgetSpacing(4, 0, 4, 4);
			//Content.MinWidth = 30;
			//Content.MinHeight = 200;
			//Content.NaturalWidth = 300;
			var button = new Xwt.Button("Test");
			Content.PackStart(button, Xwt.BoxMode.Fill);
			Content.PackStart(new Xwt.RichTextView(), Xwt.BoxMode.Fill);
			Content.PackStart(new Xwt.ComboBox(), Xwt.BoxMode.Fill);
			//Content.AddChild(button);
		}
	}

}
