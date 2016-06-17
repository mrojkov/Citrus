using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lime
{
	public class Focusable
	{
		public int TabOrder { get; set; }
	}

	/// <summary>
	/// Controls switching of focus between widgets based on keyboard shortcuts.
	/// </summary>
	public class KeyboardFocusController
	{
		public readonly Widget Widget;

		public static Widget Focused { get; private set; }

		static KeyboardFocusController()
		{
			Input.KeyTranslators.Add(new ShortcutTranslator(Key.Tab, Key.FocusNext));
			Input.KeyTranslators.Add(new ShortcutTranslator(new Shortcut(Modifiers.Shift, Key.Tab), Key.FocusPrevious));
		}

		public static void SetFocus(Widget widget)
		{
			if (Focused == widget) {
				return;
			}
			if (Focused != null) {
				Focused.Input.Release();
			}
			if (widget != null) {
				widget.Input.Capture(KeySets.Keyboard);
			}
			Focused = widget;
		}

		public KeyboardFocusController(Widget widget)
		{
			Widget = widget;
			widget.Tasks.Add(FocusTask());
		}

		private IEnumerator<object> FocusTask()
		{
			while (true) {
				if (Widget.Input.WasKeyRepeated(Key.FocusNext)) {
					AdvanceFocus(1);
				} else if (Widget.Input.WasKeyRepeated(Key.FocusPrevious)) {
					AdvanceFocus(-1);
				}
				yield return null;
			}
		}

		private void AdvanceFocus(int direction)
		{
			if (Focused != null && Focused.DescendantOrThis(Widget)) {
				var allFocusables = GetAllFocusables().ToList();
				var i = allFocusables.IndexOf(Focused);
				if (i >= 0) {
					i = Mathf.Wrap(i + direction, 0, allFocusables.Count - 1);
					SetFocus(allFocusables[i]);
				}
			}
		}

		private IEnumerable<Widget> GetAllFocusables()
		{
			return Widget.Descendants.OfType<Widget>().Where(i => i.Focusable != null).OrderBy(i => i.Focusable.TabOrder);
		}
	}
}
