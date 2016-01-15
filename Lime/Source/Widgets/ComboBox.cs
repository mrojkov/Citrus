using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class ComboBox : Widget
	{
		private int index = -1;

		public readonly Widget TextWidget;
		public readonly ObservableList<Item> Items;

		public int Index
		{
			get { return index; }
			set
			{
				index = value;
				Items_Changed();
			}
		}

		public string Text
		{
			get { return Index == -1 ? null : Items[Index].Text; }
			set
			{
				var item = Items.FirstOrDefault(i => i.Text == Text);
				Index = (item != null) ? Items.IndexOf(item) : -1;
			}
		}

		public ComboBox()
		{
			Items = new ObservableList<Item>();
			(Items as INotifyListChanged).Changed += Items_Changed;

			TextWidget = new SimpleText();
			// Show dropdown list on mouse press.
			TextWidget.Updated += (delta) => {
				if (Input.WasMousePressed() && TextWidget.HitTest(Input.MousePosition)) {
#if MAC
					Window.Current.Input.SetKeyState(Key.Mouse0, false);
#endif
					ShowDropDownList();
				}
			};
			AddNode(TextWidget);
			Theme.Current.Apply(this);
		}

		private void ShowDropDownList()
		{
#if MAC
			var menu = new Menu();
			int j = 0;
			IMenuItem selectedItem = null;
			foreach (var i in Items) {
				var menuItem = new MenuItem { Text = i.Text };
				if (j == Index) {
					selectedItem = menuItem;
				}
				var t = j;
				menuItem.Clicked += () => Index = t;
				menu.Add(menuItem);
				j++;
			}
			var aabb = CalcAABBInWindowSpace();
			menu.Popup(Window.Current, aabb.A, aabb.Width, selectedItem);
#else
			throw new NotImplementedException();
#endif
		}

		private void Items_Changed()
		{
			TextWidget.Text = Text;
		}

		public class Item
		{
			public string Text;
			public object Value;

			public Item(string text)
			{
				Text = text;
				Value = text;
			}

			public Item(string text, object value)
			{
				Text = text;
				Value = value;
			}
		}
	}
}