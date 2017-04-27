using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lime
{
	public abstract class CommonDropDownList : Widget
	{
		private int index = -1;
		public event Action<int> Changed;
		public readonly ObservableCollection<Item> Items = new ObservableCollection<Item>();
		public NodeReference<Widget> LabelRef { get; set; } = new NodeReference<Widget>("Label");
		
		public Widget Label
		{
			get { return LabelRef.Node; }
			set { LabelRef = new NodeReference<Widget>(value); }
		}

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

		public CommonDropDownList()
		{
			Input.AcceptMouseBeyondWidget = false;
			Items.CollectionChanged += (sender, e) => RefreshLabel();
			HitTestTarget = true;
		}

		protected override void Awake()
		{
			base.Awake();
			Tasks.Add(Loop());
		}

		protected override void RefreshReferences()
		{
			LabelRef = LabelRef.Resolve(Parent);
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
				if (
					ShouldHandleSpacebar() && Input.ConsumeKeyPress(Key.Space) ||
					Input.ConsumeKeyPress(Key.Up) ||
					Input.ConsumeKeyPress(Key.Down))
				{
					ShowDropDownList();
				} else if (Input.ConsumeKeyPress(Key.Escape) || Input.ConsumeKeyPress(Key.Enter)) {
					RevokeFocus();
				}
				yield return Task.WaitForInput();
			}
		}
		
		protected abstract bool ShouldHandleSpacebar();

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
				var command = new Command(i.Text);
				if (j == Index) {
					selectedCommand = command;
				}
				var t = j;
				command.Issued += () => {
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
			if (Label != null) {
				Label.Text = Text;
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

	public class DropDownList : CommonDropDownList
	{
		public DropDownList()
		{
			Theme.Current.Apply(this);
		}

		protected override bool ShouldHandleSpacebar() => true;
	}

	public class ComboBox : CommonDropDownList
	{
		public ComboBox()
		{
			Theme.Current.Apply(this);
		}
		
		protected override bool ShouldHandleSpacebar() => false;
	}
}