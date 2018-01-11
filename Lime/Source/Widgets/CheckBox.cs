using System;
using System.Collections.Generic;

namespace Lime
{
	public class CheckBox : Widget
	{
		private bool @checked;

		public event Action<ChangedEventArgs> Changed;

		public bool Checked
		{
			get { return @checked; }
			set { SetChecked(value); }
		}

		public CheckBox()
		{
			Input.AcceptMouseBeyondWidget = false;
		}

		protected override void Awake()
		{
			LateTasks.Add(Loop());
		}

		IEnumerator<object> Loop()
		{
			var button = TryFind<Widget>("Button");
			while (true) {
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
			SetChecked(!@checked, changedByUser);
		}

		private void SetChecked(bool @checked, bool changedByUser = false)
		{
			if (this.@checked != @checked) {
				this.@checked = @checked;
				RiseChanged(changedByUser);
				Window.Current.Invalidate();
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