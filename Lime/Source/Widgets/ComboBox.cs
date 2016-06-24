using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace Lime
{
	public class ComboBox : Widget
	{
		private Widget label;
		private int index = -1;

		public readonly ObservableCollection<Item> Items;

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
				var item = Items.FirstOrDefault(i => i.Text == value);
				Index = (item != null) ? Items.IndexOf(item) : -1;
			}
		}

		public object Value
		{
			get { return Index == -1 ? null : Items[Index].Value; }
			set
			{
				var item = Items.FirstOrDefault(i => i.Value.Equals(value));
				Index = (item != null) ? Items.IndexOf(item) : -1;
			}
		}

		public ComboBox()
		{
			Items = new ObservableCollection<Item>();
			Items.CollectionChanged += (sender, e) => RefreshLabel();
			HitTestTarget = true;
			Theme.Current.Apply(this);
		}

		protected override void Awake()
		{
			RefreshLabel();
			Updated += delta => {
				if (Input.WasMousePressed() && IsMouseOver()) {
					KeyboardFocus.Instance.SetFocus(this);
#if MAC
					Window.Current.Input.SetKeyState(Key.Mouse0, false);
#endif
					ShowDropDownList();
				}
				if (Input.WasKeyPressed(Key.Space) || Input.WasKeyPressed(Key.Enter)) {
					ShowDropDownList();
				} else if (Input.WasKeyRepeated(Key.Up)) {
					Index = Math.Max(0, Index - 1);
				} else if (Input.WasKeyRepeated(Key.Down)) {
					Index = Math.Min(Items.Count - 1, Index + 1);
				}
			};
		}

		private void ShowDropDownList()
		{
#if MAC || WIN
			var menu = new Menu();
			int j = 0;
			ICommand selectedCommand = null;
			foreach (var i in Items) {
				var command = new Command { Text = i.Text };
				if (j == Index) {
					selectedCommand = command;
				}
				var t = j;
				command.Executing += () => Index = t;
				menu.Add(command);
				j++;
			}
			var aabb = CalcAABBInWindowSpace();
			menu.Popup(Window.Current, aabb.A, aabb.Width, selectedCommand);
#else
			throw new NotImplementedException();
#endif
		}

		private void RefreshLabel()
		{
			label = TryFind<SimpleText>("Label");
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