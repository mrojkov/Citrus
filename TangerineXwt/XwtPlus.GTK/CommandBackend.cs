using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XwtPlus.GtkBackend
{
	class CommandBackend : ICommandBackend
	{
		Command frontend;

		static Gtk.AccelGroup accelGroup;

		public void InitializeBackend(object frontend, Xwt.Backends.ApplicationContext context)
		{
			this.frontend = frontend as XwtPlus.Command;
		}

		public void Realize()
		{
			if (frontend.Context != null) {
				frontend.Context.KeyPressed += widget_KeyPressed;
			}
			foreach (var mi in frontend.MenuItems) {
				Bind(mi);
			}
		}

		public void Bind(XwtPlus.MenuItem menuItem)
		{
			SetMenuItemAccelerator(menuItem);
			menuItem.Clicked += menuItem_Clicked;
			OnCommandChanged();
		}

		private void SetMenuItemAccelerator(XwtPlus.MenuItem menuItem)
		{
			if (accelGroup == null) {
				accelGroup = new Gtk.AccelGroup();
			}
			foreach (var w in Gtk.Window.ListToplevels()) {
				w.RemoveAccelGroup(accelGroup);
				w.AddAccelGroup(accelGroup);
			}
			var accel = AcceleratorConverter.FromString(frontend.KeySequence);
			var gtkMenuItem = menuItem.Backend.GetNativeMenuItem() as Gtk.MenuItem;
			gtkMenuItem.AddAccelerator("activate", accelGroup, accel);
		}

		public void OnCommandChanged()
		{
			foreach (var item in frontend.MenuItems) {
				item.Label = frontend.Text;
				item.Visible = frontend.Visible;
				item.Sensitive = frontend.Sensitive;
			}
		}

		void menuItem_Clicked(object sender, EventArgs e)
		{
			frontend.OnTriggered();
		}

		void widget_KeyPressed(object sender, Xwt.KeyEventArgs e)
		{
			if (frontend.KeySequence == e.Key.ToString()) {
				frontend.OnTriggered();
			} else if (frontend.KeySequence == "Backspace" && e.Key == Xwt.Key.BackSpace) {
				frontend.OnTriggered();
			}
		}

		void Xwt.Backends.IBackend.EnableEvent(object eventId)
		{
		}

		void Xwt.Backends.IBackend.DisableEvent(object eventId)
		{
		}
	}
}
