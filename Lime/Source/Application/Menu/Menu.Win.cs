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
			foreach (var menuItem in items) {
				var ni = menuItem.NativeItem;
				ni.Width = NativeContextMenu.Width;
				if (menuItem == command) {
					ni.Select();
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
			NativeItem.ShortcutKeys = ToNativeKeys(Command.Shortcut);
			if (Command.Submenu != null) {
				Command.Submenu.Refresh();
				NativeItem.DropDown = Command.Submenu.NativeContextMenu;
			} else {
				NativeItem.DropDown = null;
			}
		}

		private static Keys ToNativeKeys(Shortcut shortcut)
		{
			return ToNativeKeys(shortcut.Modifiers) | ToNativeKeys(shortcut.Main);
		}

		private static readonly Func<Keys> InvalidKeyExceptionFunc = () => { throw new ArgumentException(); };
		private static Keys ToNativeKeys(Key key)
		{
			if (key == Key.Unknown) {
				return Keys.None;
			}
			if (key >= Key.A && key <= Key.Z) {
				return Keys.A + key - Key.A;
			}
			if (key >= Key.Number0 && key <= Key.Number9) {
				return Keys.D0 + key - Key.Number0;
			}
			if (key >= Key.F1 && key <= Key.F12) {
				return Keys.F1 + key - Key.F1;
			}
			return key == Key.Up ? Keys.Up :
				key == Key.Down ? Keys.Down :
				key == Key.Left ? Keys.Left :
				key == Key.Right ? Keys.Right :
				key == Key.Enter ? Keys.Enter :
				key == Key.Escape ? Keys.Escape :
				key == Key.Space ? Keys.Space :
				key == Key.Tab ? Keys.Tab :
				key == Key.Back ? Keys.Back :
				key == Key.BackSpace ? Keys.Back :
				key == Key.Insert ? Keys.Insert :
				key == Key.Delete ? Keys.Delete :
				key == Key.PageUp ? Keys.PageUp :
				key == Key.PageDown ? Keys.PageDown :
				key == Key.Home ? Keys.Home :
				key == Key.End ? Keys.End :
				key == Key.CapsLock ? Keys.CapsLock :
				key == Key.ScrollLock ? Keys.Scroll :
				key == Key.PrintScreen ? Keys.PrintScreen :
				key == Key.Pause ? Keys.Pause :
				InvalidKeyExceptionFunc();
		}

		private static Keys ToNativeKeys(Modifiers modifiers)
		{
			var keys = Keys.None;
			if ((modifiers & Modifiers.Control) != 0) {
				keys |= Keys.Control;
			}
			if ((modifiers & Modifiers.Shift) != 0) {
				keys |= Keys.Shift;
			}
			if ((modifiers & Modifiers.Alt) != 0) {
				keys |= Keys.Alt;
			}
			if ((modifiers & Modifiers.Command) != 0) {
				keys |= Keys.Control;
			}
			return keys;
		}
	}
}
#endif
