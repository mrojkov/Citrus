using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class ComboBox : Widget
	{
		private Widget label;
		private int index = -1;
		public readonly ObservableList<Item> Items;

		public int Index
		{
			get { return index; }
			set
			{
				index = value;
				RefreshLabel();
			}
		}

		public override string Text
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
			(Items as INotifyListChanged).Changed += RefreshLabel;
			Theme.Current.Apply(this);
		}

		protected override void Awake()
		{
			label = this["Label"];
			RefreshLabel();
			// Show dropdown list on mouse press.
			label.Updated += delta => {
				if (Input.WasMousePressed() && label.HitTest(Input.MousePosition)) {
#if MAC
					Window.Current.Input.SetKeyState(Key.Mouse0, false);
#endif
					ShowDropDownList();
				}
			};
		}

		private void ShowDropDownList()
		{
#if MAC || WIN
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

		private void RefreshLabel()
		{
			if (label != null) {
				label.Text = Text;
			}
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