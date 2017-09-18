#if WIN
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public class Menu : List<ICommand>, IMenu
	{
		List<MenuItem> items = new List<MenuItem>();

		private MenuStrip nativeMainMenu;
		private ContextMenuStrip nativeContextMenu;
		private bool displayCheckMark;
		public bool DisplayCheckMark
		{
			get
			{
				return displayCheckMark;
			}
			set
			{
				displayCheckMark = value;
				NativeContextMenu.ShowImageMargin = value;
			}
		}
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
						ShowImageMargin = DisplayCheckMark
					};
					UpdateNativeMenu(nativeContextMenu);
				}
				return nativeContextMenu;
			}
		}

		private IEnumerable<ICommand> AllCommands()
		{
			foreach (var i in this) {
				yield return i;
			}
			foreach (var i in this) {
				if (i.Menu != null) {
					foreach (var j in ((Menu)i.Menu).AllCommands()) {
						yield return j;
					}
				}
			}
		}

		public ICommand FindCommand(string text)
		{
			return AllCommands().First(i => i.Text == text);
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
			var w = Window.Current as Window;
			w.Input.ClearKeyState();
			NativeContextMenu.Show(w.Form, w.WorldToWindow(w.Input.MousePosition));
		}

		public void Popup(IWindow window, Vector2 position, float minimumWidth, ICommand command)
		{
			Refresh();
			window.Input.ClearKeyState();
			NativeContextMenu.MinimumSize = new System.Drawing.Size(
				(int)minimumWidth, NativeContextMenu.MinimumSize.Height);
			foreach (var menuItem in items) {
				var ni = menuItem.NativeItem;
				ni.AutoSize = false;
				ni.Width = NativeContextMenu.Width;
				if (menuItem.Command == command) {
					ni.Select();
				}
			}
			NativeContextMenu.Show(window.Form, (window as Window).WorldToWindow(position));
		}
	}

	class MenuItem
	{
		private int commandVersion;
		public readonly ICommand Command;
		public readonly ToolStripItem NativeItem;

		public MenuItem(ICommand command)
		{
			Command = command;
			if (command == Lime.Command.MenuSeparator) {
				NativeItem = new ToolStripSeparator();
			} else {
				NativeItem = new ToolStripMenuItem();
				Command.Issued += () => {
					((ToolStripMenuItem)NativeItem).Checked = Command.Checked;
				};
				NativeItem.Click += (s, e) => CommandQueue.Instance.Add((Command)Command);

			}
			Refresh();
		}

		public void Refresh()
		{
			if (Command.Menu != null) {
				Command.Menu.Refresh();
			}
			if (Command.Version == commandVersion) {
				return;
			}
			commandVersion = Command.Version;
			NativeItem.Visible = Command.Visible;
			NativeItem.Enabled = Command.Enabled;
			NativeItem.Text = Command.Text;
			var mi = NativeItem as ToolStripMenuItem;
			if (mi == null)
				return;
			mi.ShortcutKeys = ToNativeKeys(Command.Shortcut);
			mi.Checked = Command.Checked;
			if (Command.Menu != null) {
				mi.DropDown = ((Menu)Command.Menu).NativeContextMenu;
			} else {
				mi.DropDown = null;
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
			if ((modifiers & Modifiers.Alt) != 0) {
				keys |= Keys.Alt;
			}
			if ((modifiers & Modifiers.Control) != 0) {
				keys |= Keys.Control;
			}
			if ((modifiers & Modifiers.Shift) != 0) {
				keys |= Keys.Shift;
			}
			return keys;
		}
	}
}
#endif
