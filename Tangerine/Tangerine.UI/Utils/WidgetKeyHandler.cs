using System;
using System.Collections.Generic;
using System.Linq;
using Lime;

namespace Tangerine.UI
{
	public class WidgetKeyHandler
	{
		private Widget widget;
		private Key key;

		public event Action KeyPressed;
		public event Action KeyReleased;
		public event Action KeyRepeated;

		public WidgetKeyHandler(Widget widget, Key key)
		{
			this.widget = widget;
			this.key = key;
			var keys = (widget.FocusOptions = widget.FocusOptions ?? new FocusOptions());
			keys.WantedKeys.Set(key, true);
			widget.Tasks.Add(MainTask());
		}

		private IEnumerator<object> MainTask()
		{
			while (true) {
				if (KeyPressed != null && widget.Input.WasKeyPressed(key)) {
					KeyPressed.Invoke();
				}
				if (KeyReleased != null && widget.Input.WasKeyReleased(key)) {
					KeyReleased.Invoke();
				}
				if (KeyRepeated != null && widget.Input.WasKeyRepeated(key)) {
					KeyRepeated.Invoke();
				}
				yield return Task.WaitForInput();
			}
		}
	}
}