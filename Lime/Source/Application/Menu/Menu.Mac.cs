#if MAC
using System;
using System.Linq;
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
			NativeMenu.AutoEnablesItems = false;
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

		public ICommand FindCommand(string text)
		{
			return AllCommands().First(i => i.Text == text);
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
			if (NSApplication.SharedApplication.ModalWindow != null) {
				// Fixme: When a container dialog is running in the modal mode, showing the menu with PopUpMenu() causes all menu items are disabled.
				// So, use PopUpContextMenu() instead as a workaround.
				var evt = NSApplication.SharedApplication.CurrentEvent;
				NSMenu.PopUpContextMenu(NativeMenu, evt, CommonWindow.Current.NSGameView);
			} else {
				NativeMenu.MinimumWidth = minimumWidth;
				NSMenuItem item = command == null ? null : items.Find(i => i.Command == command).NativeMenuItem;
				NativeMenu.PopUpMenu(item, new CoreGraphics.CGPoint(position.X, window.ClientSize.Y - position.Y), window.NSGameView);
			}
		}

		class MenuItem
		{
			private bool separator;
			private CommandState state;
			public readonly ICommand Command;
			public readonly NSMenuItem NativeMenuItem;

			public MenuItem(ICommand command)
			{
				Command = command;
				if (command == Lime.Command.MenuSeparator) {
					NativeMenuItem = NSMenuItem.SeparatorItem;
					separator = true;
				} else {
					NativeMenuItem = new NSMenuItem();
					NativeMenuItem.Activated += (s, e) => {
						Command.Execute();
					};
					Refresh();
				}
			}

			public void Refresh()
			{
				if (separator) {
					return;
				}
				if (Command.Submenu != null) {
					Command.Submenu.Refresh();
				}
				if (state != null && state.Equals(Command)) {
					return;
				}
				state = new CommandState(Command);
				NativeMenuItem.Hidden = !Command.Visible;
				NativeMenuItem.Enabled = Command.Enabled;
				NativeMenuItem.Title = Command.Text;
				if (Command.Shortcut.Main != Key.Unknown) {
					NativeMenuItem.KeyEquivalent = GetKeyEquivalent(Command.Shortcut.Main);
					NativeMenuItem.KeyEquivalentModifierMask = GetModifierMask(Command.Shortcut.Modifiers);
				} else {
					NativeMenuItem.KeyEquivalent = "";
					NativeMenuItem.KeyEquivalentModifierMask = 0;
				}
				if (Command.Submenu != null) {
					var nativeSubmenu = Command.Submenu.NativeMenu;
					nativeSubmenu.Title = Command.Text;
					NativeMenuItem.Submenu = nativeSubmenu;
				} else {
					NativeMenuItem.Submenu = null;
				}
			}

			static string GetKeyEquivalent(Key key)
			{
				if (key >= Key.A && key <= Key.Z) {
					return ((char)('a' + key - Key.A)).ToString();
				}
				if (key >= Key.Number0 && key <= Key.Number9) {
					return ((char)('0' + key - Key.A)).ToString();
				}
				if (key == Key.Tab) {
					return "\t";
				}
				if (key == Key.Enter) {
					return "\r";
				}
				if (key == Key.Delete) {
					return ((char)127).ToString();
				}
				throw new ArgumentException();
			}

			static NSEventModifierMask GetModifierMask(Modifiers modifiers)
			{
				NSEventModifierMask result = 0;
				if ((modifiers & Modifiers.Shift) != 0) {
					result |= NSEventModifierMask.ShiftKeyMask;
				}
				if ((modifiers & Modifiers.Alt) != 0) {
					result |= NSEventModifierMask.AlternateKeyMask;
				}
				if ((modifiers & Modifiers.Control) != 0) {
					result |= NSEventModifierMask.ControlKeyMask;
				}
				if ((modifiers & Modifiers.Command) != 0) {
					result |= NSEventModifierMask.CommandKeyMask;
				}
				return result;
			}

			class CommandState
			{
				readonly string Text;
				readonly Shortcut Shortcut;
				readonly Menu Submenu;
				readonly bool Enabled;
				readonly bool Visible;

				public CommandState(ICommand command)
				{
					Text = command.Text;
					Shortcut = command.Shortcut;
					Submenu = command.Submenu;
					Enabled = command.Enabled;
					Visible = command.Visible;
				}

				public bool Equals(ICommand command)
				{
					return
						Text == command.Text &&
						Shortcut == command.Shortcut &&
						Submenu == command.Submenu &&
						Enabled == command.Enabled &&
						Visible == command.Visible;
				}
			}
		}
	}
}
#endif