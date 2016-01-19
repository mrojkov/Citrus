#if WIN
using System;
using System.Windows.Forms;
using SD = System.Drawing;

namespace Lime
{
	class Menu : ObservableList<IMenuItem>, IMenu
	{
		private readonly ContextMenuStrip menu;
		private bool validated = true;

		public Menu()
		{
			menu = new ContextMenuStrip {
				ShowImageMargin = false
			};
		}

		public void Popup()
		{
			Validate();
			menu.Show(Window.Current.Form, new SD.Point());
		}

		public void Popup(IWindow window, Vector2 position, float minimumWidth, IMenuItem item)
		{
			Validate();
			menu.MinimumSize = new SD.Size((int)minimumWidth, menu.MinimumSize.Height);
			foreach (var menuItem in this) {
				var mi = ((MenuItem)menuItem).Item;
				mi.Width = menu.Width;
				if (menuItem == item) {
					mi.Select();
				}
			}
			menu.Show(window.Form, new SD.Point((int)position.X, (int)position.Y));
		}

		protected override void OnChanged()
		{
			validated = false;
			base.OnChanged();
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

	class MenuItem : IMenuItem
	{
		public MenuItem()
		{
			Item = new ToolStripMenuItem {
				AutoSize = false
			};
			Item.Click += Item_Click;
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

		private void Item_Click(object sender, EventArgs e)
		{
			if (Clicked != null) {
				Clicked();
			}
		}
	}
}
#endif
