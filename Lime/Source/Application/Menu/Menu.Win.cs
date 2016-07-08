#if WIN
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using SD = System.Drawing;

namespace Lime
{
	public class Menu : List<ICommand>, IMenu
	{
		List<MenuItem> items = new List<MenuItem>();

		private MenuStrip nativeMainMenu;
		private ContextMenuStrip nativeContextMenu;

		internal MenuStrip NativeMainMenu
		{
			get
			{
				if (nativeMainMenu == null) {
					nativeMainMenu = new MenuStrip();
					UpdateNativeMenu(nativeMainMenu);
				}
				return nativeMainMenu;
			}
		}

		internal ContextMenuStrip NativeContextMenu
		{
			get
			{
				if (nativeContextMenu == null) {
					nativeContextMenu = new ContextMenuStrip {
						ShowImageMargin = false
					};
					UpdateNativeMenu(nativeContextMenu);
				}
				return nativeContextMenu;
			}
		}

		private void Rebuild()
		{
			items.Clear();
			foreach (var i in this) {
				items.Add(new MenuItem(i));
			}
			if (nativeMainMenu != null) {
				UpdateNativeMenu(nativeMainMenu);
			}
			if (nativeContextMenu != null) {
				UpdateNativeMenu(nativeContextMenu);
			}
		}

		private void UpdateNativeMenu(ToolStrip menu)
		{
			menu.Items.Clear();
			foreach (var i in items) {
				menu.Items.Add(i.NativeItem);
			}
		}

		public void Refresh()
		{
			if (items.Count != Count) {
				Rebuild();
				return;
			}
			int j = 0;
			foreach (var i in items) {
				if (i.Command != this[j++]) {
					Rebuild();
					break;
				}
				i.Refresh();
			}
		}

		public void Popup()
		{
			Refresh();
			NativeContextMenu.Show(Window.Current.Form, new SD.Point());
		}

		public void Popup(IWindow window, Vector2 position, float minimumWidth, ICommand command)
		{
			Refresh();
			NativeContextMenu.MinimumSize = new SD.Size(
				(int)minimumWidth, NativeContextMenu.MinimumSize.Height);
			foreach (var menuItem in this) {
				var mi = ((MenuItem)menuItem).NativeItem;
				mi.Width = NativeContextMenu.Width;
				if (menuItem == command) {
					mi.Select();
				}
			}
			NativeContextMenu.Show(window.Form, new SD.Point((int)position.X, (int)position.Y));
		}
	}

	class MenuItem
	{
		public readonly ICommand Command;
		public readonly ToolStripMenuItem NativeItem;

		public MenuItem(ICommand command)
		{
			Command = command;
			NativeItem = new ToolStripMenuItem();
			NativeItem.Click += (s, e) => command.Execute();
			Refresh();
		}

		public void Refresh()
		{
			Command.Refresh();
			NativeItem.Visible = Command.Visible;
			NativeItem.Enabled = Command.Enabled;
			NativeItem.Text = Command.Text;
			if (Command.Submenu != null) {
				Command.Submenu.Refresh();
				NativeItem.DropDown = Command.Submenu.NativeContextMenu;
			} else {
				NativeItem.DropDown = null;
			}
		}
	}
}
#endif
