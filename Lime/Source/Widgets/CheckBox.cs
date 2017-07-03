using System;
using System.Collections.Generic;

namespace Lime
{
	public class CheckBox : Widget
	{
		private bool @checked;

		public event Action<bool> Changed;

		public bool Checked
		{
			get { return @checked; }
			set
			{
				@checked = value;
				Window.Current.Invalidate();
			}
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
					Toggle();
				}
				if (Input.ConsumeKeyPress(Key.Space)) {
					Toggle();
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
			if (Changed != null) {
				Changed(Checked);
			}
		}
	}
}