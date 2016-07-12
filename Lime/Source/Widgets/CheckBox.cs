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
			Theme.Current.Apply(this);
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
				if (IsFocused()) {
					if (Input.WasKeyPressed(Key.Space)) {
						Toggle();
					}
					if (Input.WasKeyPressed(Key.Enter) || Input.WasKeyPressed(Key.Escape)) {
						RevokeFocus();
					}
				}
				yield return Task.WaitForInput();
			}
		}

		void Toggle()
		{
			Checked = !Checked;
			if (Changed != null) {
				Changed(Checked);
			}
		}
	}
}