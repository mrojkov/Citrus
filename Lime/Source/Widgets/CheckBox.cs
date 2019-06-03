using System;
using System.Collections.Generic;

namespace Lime
{
	public enum CheckBoxState
	{
		Unchecked,
		Checked,
		Indeterminate
	}

	[YuzuDontGenerateDeserializer]
	public class CheckBox : Widget
	{
		private CheckBoxState state;
		public CheckBoxState State
		{
			get => state;
			set => SetState(value);
		}

		private void SetState(CheckBoxState value, bool changedByUser = false)
		{
			if (state != value) {
				state = value;
				RiseChanged(changedByUser);
				Window.Current.Invalidate();
			}
		}

		public event Action<ChangedEventArgs> Changed;

		public bool Checked
		{
			get { return State == CheckBoxState.Checked; }
			set { SetChecked(value); }
		}

		public CheckBox()
		{
			Input.AcceptMouseBeyondWidget = false;
			Awoke += n => n.LateTasks.Add(((CheckBox)n).Loop());
		}

		private IEnumerator<object> Loop()
		{
			var button = TryFind<Widget>("Button");
			button.Clicked += () => Clicked?.Invoke();
			while (true) {
				if (!GloballyEnabled) {
					yield return null;
				}
				if (button != null && button.WasClicked()) {
					SetFocus();
					ToggleInternal(true);
				}
				if (Input.ConsumeKeyPress(Key.Space)) {
					ToggleInternal(true);
				}
				if (Input.ConsumeKeyPress(Key.Enter) || Input.ConsumeKeyPress(Key.Escape)) {
					RevokeFocus();
				}
				yield return Task.WaitForInput();
			}
		}

		public void Toggle()
		{
			Checked = !Checked;
		}

		private void ToggleInternal(bool changedByUser = false)
		{
			SetChecked(!Checked, changedByUser);
		}

		private void SetChecked(bool @checked, bool changedByUser = false)
		{
			if (@checked && State != CheckBoxState.Checked) {
				SetState(CheckBoxState.Checked, changedByUser);
			} else if (!@checked && State != CheckBoxState.Unchecked) {
				SetState(CheckBoxState.Unchecked, changedByUser);
			}
		}

		protected void RiseChanged(bool changedByUser = false)
		{
			Changed?.Invoke(new ChangedEventArgs { Value = Checked, ChangedByUser = changedByUser });
		}

		public class ChangedEventArgs
		{
			public bool Value;
			public bool ChangedByUser;
		}
	}
}
