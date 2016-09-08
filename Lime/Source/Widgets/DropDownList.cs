using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lime
{
	public class DropDownList : Widget
	{
		private int index = -1;
		public event Action<int> Changed;
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

		public DropDownList()
		{
			Input.AcceptMouseBeyondWidget = false;
			Items = new ObservableCollection<Item>();
			Items.CollectionChanged += (sender, e) => RefreshLabel();
			HitTestTarget = true;
			Theme.Current.Apply(this);
		}

		protected override void Awake()
		{
			Tasks.Add(Loop());
		}

		IEnumerator<object> Loop()
		{
			RefreshLabel();
			while (true) {
				if (Input.WasMousePressed() && IsMouseOver()) {
					SetFocus();
#if MAC
					Window.Current.Input.SetKeyState(Key.Mouse0, false);
#endif
					yield return ShowDropDownListTask();
				}
				if (Input.ConsumeKeyPress(Key.Space) || Input.ConsumeKeyPress(Key.Up) || Input.ConsumeKeyPress(Key.Down)) {
					ShowDropDownList();
				} else if (Input.ConsumeKeyPress(Key.Escape) || Input.ConsumeKeyPress(Key.Enter)) {
					RevokeFocus();
				}
				yield return Task.WaitForInput();
			}
		}

		IEnumerator<object> ShowDropDownListTask()
		{
			yield return null;
			ShowDropDownList();
		}

		private void ShowDropDownList()
		{
#if MAC || WIN
			var menu = new Menu();
			int j = 0;
			ICommand selectedCommand = null;
			foreach (var i in Items) {
				var command = new DelegateCommand(i.Text);
				if (j == Index) {
					selectedCommand = command;
				}
				var t = j;
				command.Executing += () => {
					Index = t; 
					if (Changed != null) {
						Changed(Index);
					}
				};
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
			var label = TryFind<SimpleText>("Label");
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