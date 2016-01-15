#if MAC
using System;
using AppKit;

namespace Lime
{
	public class MenuItem : IMenuItem
	{
		public NSMenuItem NSMenuItem { get; private set; }
		public event Action Clicked;

		public bool Visible
		{
			get { return !NSMenuItem.Hidden; }
			set { NSMenuItem.Hidden = !value; }
		}

		public bool Enabled
		{
			get { return NSMenuItem.Enabled; }
			set { NSMenuItem.Enabled = value; }
		}

		public string Text
		{
			get { return NSMenuItem.Title; }
			set { NSMenuItem.Title = value; }
		}

		public IMenu SubMenu { get; set; }

		public MenuItem()
		{
			NSMenuItem = new NSMenuItem();
			NSMenuItem.Activated += (s, e) => {
				if (Clicked != null) {
					Clicked();
				}
			};
		}
	}

	public class Menu : ObservableList<IMenuItem>, IMenu
	{
		private NSMenu nsMenu;

		public Menu()
		{
			nsMenu = new NSMenu();
		}

		protected override void OnChanged()
		{
			nsMenu.RemoveAllItems();
			foreach (var i in this) {
				nsMenu.AddItem(((MenuItem)i).NSMenuItem);
			}
		}

		public void Popup()
		{
			var evt = NSApplication.SharedApplication.CurrentEvent;
			NSMenu.PopUpContextMenu(nsMenu, evt, Window.Current.NSGameView);
		}

		public void Popup(IWindow window, Vector2 position, float minimumWidth, IMenuItem item)
		{
			nsMenu.MinimumWidth = minimumWidth;
			NSMenuItem i = item == null ? null : ((MenuItem)item).NSMenuItem;
			nsMenu.PopUpMenu(i, new CoreGraphics.CGPoint(position.X, window.ClientSize.Height - position.Y), window.NSGameView);
		}
	}
}
#endif