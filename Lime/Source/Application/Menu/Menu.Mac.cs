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
			NativeMenu.RemoveAllItems();
			foreach (var i in this) {
				var item = new MenuItem(i);
				NativeMenu.AddItem(item.NativeMenuItem);
				items.Add(item);
			}
		}

		public void Popup()
		{
			// Repaint windows since some actions (e.g. selecting a widget) should be taken before showing the context menu.
			UpdateAndRenderAllWindows();
			Refresh();
			var evt = NSApplication.SharedApplication.CurrentEvent;
			NSMenu.PopUpContextMenu(NativeMenu, evt, CommonWindow.Current.NSGameView);
		}
		
		public void Popup(IWindow window, Vector2 position, float minimumWidth, ICommand command)
		{
			UpdateAndRenderAllWindows();
			Refresh();
			foreach (var w in Application.Windows) {
				(w as Window).HandleRenderFrame();
			}
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
		
		private void UpdateAndRenderAllWindows()
		{
			foreach (var w in Application.Windows) {
				(w as Window).HandleRenderFrame();
			}
		}

		class MenuItem
		{
			private bool separator;
			private int commandVersion;
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
					Command.Issued += () => {
						NativeMenuItem.State = Command.Checked ? NSCellStateValue.On : NSCellStateValue.Off;
					};
					NativeMenuItem.Activated += (s, e) => {
						CommandQueue.Instance.Add((Command)Command);
					};
					Refresh();
				}
			}

			public void Refresh()
			{
				if (separator) {
					return;
				}
				if (Command.Menu != null) {
					Command.Menu.Refresh();
					var e = false;
					foreach (var c in Command.Menu) {
						e |= (c != Lime.Command.MenuSeparator && c.Enabled);
					}
					NativeMenuItem.Enabled = e;
				} else {
					NativeMenuItem.Enabled = Command.Enabled;
				}
				if (commandVersion == Command.Version) {
					return;
				}
				var title = Command.Text != null ? Command.Text.Replace("&", string.Empty) : string.Empty;
				commandVersion = Command.Version;
				NativeMenuItem.Hidden = !Command.Visible;
				NativeMenuItem.Enabled = Command.Enabled;
				NativeMenuItem.Title = title;
				NativeMenuItem.State = Command.Checked ? NSCellStateValue.On : NSCellStateValue.Off;
				if (Command.Shortcut.Main != Key.Unknown) {
					NativeMenuItem.KeyEquivalent = GetKeyEquivalent(Command.Shortcut.Main);
					NativeMenuItem.KeyEquivalentModifierMask = GetModifierMask(Command.Shortcut.Modifiers);
				} else {
					NativeMenuItem.KeyEquivalent = "";
					NativeMenuItem.KeyEquivalentModifierMask = 0;
				}
				if (Command.Menu != null) {
					var nativeSubmenu = ((Menu)Command.Menu).NativeMenu;
					nativeSubmenu.Title = title;
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
				if (key >= Key.F1 && key <= Key.F12) {
					return ((char)((int)NSFunctionKey.F1 + (key.Code - Key.F1.Code))).ToString();
				}
				if (key == Key.EqualsSign) {
					return "=";
				}
				if (key == Key.Minus) {
					return "-";
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
		}
	}
}
#endif
