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
				menu.Items.Add(i.NativeItems);
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
				var mi = ((MenuItem)menuItem).NativeItems;
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
		public readonly ToolStripMenuItem NativeItems;

		public MenuItem(ICommand command)
		{
			Command = command;
			NativeItems = new ToolStripMenuItem();
			NativeItems.Click += (s, e) => command.Execute();
			Refresh();
		}

		public void Refresh()
		{
			Command.Refresh();
			NativeItems.Visible = Command.Visible;
			NativeItems.Enabled = Command.Enabled;
			NativeItems.Text = Command.Text;
			if (Command.Submenu != null) {
				Command.Submenu.Refresh();
				NativeItems.DropDown = Command.Submenu.NativeContextMenu;
			} else {
				NativeItems.DropDown = null;
			}
		}
	}
}
#endif
