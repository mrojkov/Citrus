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
		private readonly ContextMenuStrip menu;
		private bool validated = true;

		public Menu()
		{
			menu = new ContextMenuStrip {
				ShowImageMargin = false
			};
		}

		private void Rebuild()
		{
			items.Clear();
			menu.Items.Clear();
			foreach (var i in this) {
				var item = new MenuItem(i);
				menu.Items.Add(item.Item);
				items.Add(item);
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
			Validate();
			menu.Show(Window.Current.Form, new SD.Point());
		}

		public void Popup(IWindow window, Vector2 position, float minimumWidth, ICommand command)
		{
			Validate();
			menu.MinimumSize = new SD.Size((int)minimumWidth, menu.MinimumSize.Height);
			foreach (var menuItem in this) {
				var mi = ((MenuItem)menuItem).Item;
				mi.Width = menu.Width;
				if (menuItem == command) {
					mi.Select();
				}
			}
			menu.Show(window.Form, new SD.Point((int)position.X, (int)position.Y));
		}

		private void Validate()
		{
			if (!validated) {
				menu.Items.Clear();
				foreach (var item in this) {
					menu.Items.Add(((MenuItem)item).Item);
				}
				validated = true;
			}
		}
	}

	class MenuItem
	{
		public readonly ICommand Command;

		public MenuItem(ICommand command)
		{
			Command = command;
			Item = new ToolStripMenuItem {
				AutoSize = false
			};
			Item.Click += Item_Click;
			Clicked += command.Execute;
		}

		public event Action Clicked;

		public bool Enabled
		{
			get { return Item.Enabled; }
			set { Item.Enabled = value; }
		}

		public IMenu SubMenu { get; set; }

		public string Text
		{
			get { return Item.Text; }
			set { Item.Text = value; }
		}

		public ToolStripItem Item { get; private set; }

		public bool Visible
		{
			get { return Item.Visible; }
			set { Item.Visible = value; }
		}

		public void SetShortcutString(string shortcut)
		{
			((ToolStripMenuItem)Item).ShortcutKeyDisplayString = shortcut;
		}

		public void Refresh()
		{
			// TODO: implement.
			throw new NotImplementedException();
		}

		private void Item_Click(object sender, EventArgs e)
		{
			if (Clicked != null) {
				Clicked();
			}
		}
	}
}
#endif
