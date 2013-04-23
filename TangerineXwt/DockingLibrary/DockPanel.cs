using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public class DockedLocation
	{
		public Xwt.Size DockedSize;
		public DockZone DockZone;
	}

	public class FloatingLocation
	{
		public Xwt.Point Location;
		public Xwt.Size Size;
	}

	public interface IDockElement
	{
		Xwt.Widget MainWidget { get; }
	}

	public class DockPanel : IDockElement
	{
		private Xwt.Label titleLabel { get; set; }
		private Xwt.VBox panelBox;
		private Xwt.VBox contentBox;
		private Xwt.Widget content;
		private Xwt.Window window;
		private DockManager dockSiteManager;
		private bool isTitleBeingPressed;
		private DockedLocation dockedLocation;
		private FloatingLocation floatingLocation;

		Xwt.Widget IDockElement.MainWidget { get { return panelBox; } }

		public string Title
		{
			get { return titleLabel.Text; }
			set { titleLabel.Text = value; }
		}

		public bool Docked { get; private set; }
		public bool Visible { get; private set; }

		public Xwt.Widget Content
		{
			get { return content; }
			set { SetContent(value); }
		}

		public void Hide()
		{
			HidePanelAndWindow();
		}

		public void Show()
		{
			HidePanelAndWindow();
			if (Docked) {
				Dock(dockedLocation.DockZone);
			} else {
				Floating(atMouse: false);
			}
		}

		public void Floating(bool atMouse)
		{
			HidePanelAndWindow();
			CreateWindow();
			if (atMouse) {
				Xwt.Point p = XwtPlus.Utils.Instance.GetPointerPosition();
				p.X -= window.Size.Width / 2;
				p.Y -= 50;
				window.Location = p;
			}
			Docked = false;
			Visible = true;
		}

		public void Dock(DockZone zone)
		{
			HidePanelAndWindow();
			dockedLocation.DockZone = zone;
			dockSiteManager.AddDockElement(this, zone);
			Docked = true;
			Visible = true;
		}

		private void HidePanelAndWindow()
		{
			if (window != null) {
				DestroyWindow();
			}
			if (dockSiteManager.Elements.Contains(this)) {
				dockSiteManager.RemoveDockElement(this);
			}
			Visible = false;
		}

		private void SetContent(Xwt.Widget value)
		{
			contentBox.Clear();
			if (value != null) {
				contentBox.PackStart(value, Xwt.BoxMode.Expand);
			}
			content = value;
		}

		private void DestroyWindow()
		{
			if (window != null) {
				floatingLocation.Location = window.Location;
				floatingLocation.Size = window.Size;
				(window.Content as Xwt.HBox).Clear();
				window.Dispose();
				window = null;
			}
		}

		private void CreateWindow()
		{
			window = new Xwt.Window() {
				TransientFor = MainWindow.Instance.Window,
				Padding = new Xwt.WidgetSpacing(),
				Title = Title
			};
			var windowArea = new Xwt.HBox();
			window.Content = windowArea;
			windowArea.PackStart(panelBox, Xwt.BoxMode.Expand);
			window.Size = floatingLocation.Size;
			window.Location = floatingLocation.Location;
			window.Show();
		}

		void title_ButtonPressed(object sender, Xwt.ButtonEventArgs e)
		{
			if (e.Button == Xwt.PointerButton.Left) {
				isTitleBeingPressed = true;
			}
		}

		void title_MouseMoved(object sender, Xwt.MouseMovedEventArgs e)
		{
			if (isTitleBeingPressed) {
				isTitleBeingPressed = false;
				if (Docked) {
					dockSiteManager.RemoveDockElement(this);
				} else {
					DestroyWindow();
				}
				dockSiteManager.DragManager.StartDrag(this);
			}
		}

		void title_ButtonReleased(object sender, Xwt.ButtonEventArgs e)
		{
			if (e.Button == Xwt.PointerButton.Left) {
				isTitleBeingPressed = false;
			}
		}

		public DockPanel(DockManager dockSiteManager)
			: this(dockSiteManager, false, false, new DockedLocation(), new FloatingLocation())
		{
		}

		public DockPanel(
			DockManager dockSiteManager, 
			bool visible,
			bool docked, 
			DockedLocation dockedLocation,
			FloatingLocation floatingLocation)
		{
			this.dockSiteManager = dockSiteManager;
			this.dockedLocation = dockedLocation;
			this.floatingLocation = floatingLocation;
			CreateDecoration();
			this.Docked = docked;
			if (visible) {
				Show();
			}
		}

		private void CreateDecoration()
		{
			titleLabel = new Xwt.Label("DockPanel");
			titleLabel.MarginLeft = 4;
			titleLabel.MarginBottom = 2;
			titleLabel.MarginTop = 2;
			titleLabel.ButtonReleased += title_ButtonReleased;
			titleLabel.ButtonPressed += title_ButtonPressed;
			titleLabel.MouseMoved += title_MouseMoved;
			contentBox = new Xwt.VBox();
			contentBox.Margin = new Xwt.WidgetSpacing();//4, 0, 4, 4);
			panelBox = new Xwt.VBox();
			panelBox.PackStart(titleLabel, Xwt.BoxMode.Fill);
			panelBox.PackStart(contentBox, Xwt.BoxMode.Expand);
			panelBox.BackgroundColor = Colors.ToolPanelBackground;
		}
	}
}
