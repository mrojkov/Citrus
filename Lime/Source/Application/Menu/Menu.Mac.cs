#if MAC
using System;
using System.Collections.Generic;
using AppKit;

namespace Lime
{
	public class Menu : List<ICommand>, IMenu
	{
		List<MenuItem> items = new List<MenuItem>();

		internal readonly NSMenu NativeMenu;

		public Menu()
		{
			NativeMenu = new NSMenu();
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

		public IEnumerable<ICommand> AllCommands()
		{
			foreach (var i in this) {
				yield return i;
			}
			foreach (var i in this) {
				if (i.Submenu != null) {
					foreach (var j in i.Submenu.AllCommands()) {
						yield return j;
					}
				}
			}
		}

		private void Rebuild()
		{
			items.Clear();
			NativeMenu.RemoveAllItems();
			foreach (var i in this) {
				var item = new MenuItem(i);
				NativeMenu.AddItem(item.NativeMenuItem);
				items.Add(item);
			}
		}

		public void Popup()
		{
			Refresh();
			var evt = NSApplication.SharedApplication.CurrentEvent;
			NSMenu.PopUpContextMenu(NativeMenu, evt, CommonWindow.Current.NSGameView);
		}

		public void Popup(IWindow window, Vector2 position, float minimumWidth, ICommand command)
		{
			Refresh();
			NativeMenu.MinimumWidth = minimumWidth;
			NSMenuItem item = command == null ? null : items.Find(i => i.Command == command).NativeMenuItem;
			NativeMenu.PopUpMenu(item, new CoreGraphics.CGPoint(position.X, window.ClientSize.Y - position.Y), window.NSGameView);
		}

		class MenuItem
		{
			public readonly ICommand Command;
			public readonly NSMenuItem NativeMenuItem;

			public MenuItem(ICommand command)
			{
				Command = command;
				NativeMenuItem = new NSMenuItem();
				NativeMenuItem.Activated += (s, e) => command.Execute();
				Refresh();
			}

			public void Refresh()
			{
				NativeMenuItem.Hidden = !Command.Visible;
				NativeMenuItem.Enabled = Command.Enabled;
				NativeMenuItem.Title = Command.Text;
				if (Command.Shortcut.Main != Key.Unknown) {
					NativeMenuItem.KeyEquivalent = Command.Shortcut.Main.ToString().ToLower();
					NativeMenuItem.KeyEquivalentModifierMask = GetShortcutModifierMask(Command.Shortcut);
				}
				if (Command.Submenu != null) {
					var nativeSubmenu = Command.Submenu.NativeMenu;
					nativeSubmenu.Title = Command.Text;
					NativeMenuItem.Submenu = nativeSubmenu;
					Command.Submenu.Refresh();
				} else {
					NativeMenuItem.Submenu = null;
				}
			}

			static NSEventModifierMask GetShortcutModifierMask(Shortcut shortcut)
			{
				var m = shortcut.Modifier;
				if (m == Key.ShiftLeft || m == Key.ShiftRight) {
					return NSEventModifierMask.ShiftKeyMask;
				}
				if (m == Key.AltLeft || m == Key.AltRight) {
					return NSEventModifierMask.AlternateKeyMask;
				}
				if (m == Key.WinLeft || m == Key.WinRight) {
					return NSEventModifierMask.CommandKeyMask;
				}
				if (m == Key.ControlLeft || m == Key.ControlRight) {
					return NSEventModifierMask.ControlKeyMask;
				}
				return 0;
			}
		}
	}
}
#endif