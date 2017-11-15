using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lime
{
	public abstract class CommonDropDownList : Widget
	{
		public event Action<ChangedEventArgs> Changed;
		public event Action ShowingDropDownList;
		public readonly ObservableCollection<Item> Items = new ObservableCollection<Item>();
		public NodeReference<Widget> TextWidgetRef { get; set; } = new NodeReference<Widget>("TextWidget");

		public Widget TextWidget => TextWidgetRef.GetNode(this);

		private int index = -1;
		protected object userValue;

		public int Index
		{
			get { return index; }
			set
			{
				index = value;
				RefreshTextWidget();
				RaiseChanged();
			}
		}

		public override string Text
		{
			get { return Items.ElementAtOrDefault(Index)?.Text ?? (string)userValue; }
			set
			{
				var item = Items.FirstOrDefault(i => i.Text == value);
				Index = (item != null) ? Items.IndexOf(item) : -1;
				if (Index == -1) {
					userValue = value;
					RefreshTextWidget();
				} else {
					userValue = null;
				}
			}
		}

		public object Value
		{
			get { return Items.ElementAtOrDefault(Index)?.Value ?? userValue; }
			set
			{
				var item = Items.FirstOrDefault(i => i.Value.Equals(value));
				Index = (item != null) ? Items.IndexOf(item) : -1;
				userValue = Index == -1 ? value : null;
			}
		}

		public CommonDropDownList()
		{
			Input.AcceptMouseBeyondWidget = false;
			Items.CollectionChanged += (sender, e) => RefreshTextWidget();
			HitTestTarget = true;
		}

		public override Node Clone()
		{
			var clone = (CommonDropDownList)base.Clone();
			clone.TextWidgetRef = clone.TextWidgetRef?.Clone();
			return clone;
		}

		protected override void Awake()
		{
			base.Awake();
			Tasks.Add(Loop());
		}

		IEnumerator<object> Loop()
		{
			RefreshTextWidget();
			while (true) {
				if (Input.WasMousePressed() && IsMouseOver()) {
					SetFocus();
					Window.Current.Input.SetKeyState(Key.Mouse0, false);
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
			ShowingDropDownList?.Invoke();
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
					RaiseChanged();
				};
				menu.Add(command);
				j++;
			}
			var aabb = CalcAABBInWindowSpace();
			menu.Popup(Window.Current, aabb.A, aabb.Width, selectedCommand);
#if WIN
			menu.ShowImageMargin = false;
			menu.NativeContextMenu.Capture = true;
#endif
#else
			throw new NotImplementedException();
#endif
		}

		protected void RaiseChanged()
		{
			Changed?.Invoke(new ChangedEventArgs { Index = Index, Value = Value });
		}

		protected void RefreshTextWidget()
		{
			if (TextWidget != null) {
				TextWidget.Text = Text;
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

	public class ChangedEventArgs
	{
		public int Index;
		public object Value;
	}

	public class DropDownList : CommonDropDownList
	{
		protected override bool ShouldHandleSpacebar() => true;
	}

	public class ComboBox : CommonDropDownList
	{
		protected override void Awake()
		{
			((EditBox)TextWidget).Submitted += TextWidget_Submitted;
			base.Awake();
		}

		private void TextWidget_Submitted(string text)
		{
			var item = Items.FirstOrDefault(i => i.Text == text);
			if (item != null) {
				userValue = null;
				Index = Items.IndexOf(item);
			} else {
				userValue = text;
				Index = -1;
			}
			RaiseChanged();
		}

		protected override bool ShouldHandleSpacebar() => false;
	}
}