using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lime
{
	public class Focusable
	{
		public bool TabStop { get; set; }
		public int TabOrder { get; set; }
		public event Action FocusLost;
		public event Action FocusGained;

		public Focusable()
		{
			TabStop = true;
		}

		internal void RaiseFocusLost()
		{
			FocusLost?.Invoke();
		}

		internal void RaiseFocusGained()
		{
			FocusGained?.Invoke();
		}
	}

	public class FocusOptions
	{
		public readonly BitArray WantedKeys;

		public FocusOptions()
		{
			WantedKeys = new BitArray(Key.MaxCount);
		}
	}

	public class KeyboardFocus
	{
		private Dictionary<IWindow, Widget> focusPerWindow = new Dictionary<IWindow, Widget>();
		private BitArray capturedKeys = new BitArray(Key.MaxCount);

		public static KeyboardFocus Instance { get; private set; }
		public Widget Focused { get; private set; }

		public static void Initialize()
		{
			Instance = new KeyboardFocus();
		}

		private KeyboardFocus()
		{
			Application.Windows.CollectionChanged += (sender, e) => {
				if (e.NewItems != null) {
					foreach (var i in e.NewItems) {
						var window = (IWindow)i; 
						window.Deactivated += () => Window_Deactivated(window);
						window.Activated += () => Window_Activated(window);
					}
				}
				if (e.OldItems != null) {
					foreach (var i in e.OldItems) {
						focusPerWindow.Remove((IWindow)i);
					}
				}
			};
		}

		private void Window_Deactivated(IWindow window)
		{
			if (Focused != null && Focused.GetRoot() == WidgetContext.Current.Root) {
				focusPerWindow[window] = Focused;
			} else if (Focused == null) {
				focusPerWindow[window] = null;
			}
		}

		private void Window_Activated(IWindow window)
		{
			Widget newFocused;
			if (focusPerWindow.TryGetValue(window, out newFocused)) {
				if (newFocused != null && newFocused.GetRoot() == WidgetContext.Current.Root) {
					SetFocus(newFocused);
					return;
				}
			}
			SetFocus(null);
		}

		public void SetFocus(Widget value)
		{
			if (Focused == value) {
				return;
			}
			if (Focused != null) {
				Focused.Input.Release(capturedKeys);
				if (Focused != null && Focused.Focusable != null) {
					Focused.Focusable.RaiseFocusLost();
				}
			}
			if (value != null) {
				capturedKeys = GetKeysToCapture(value);
				value.Input.Capture(capturedKeys);
				Application.SoftKeyboard.Show(true, value.Text);
				if (Focused != null && Focused.Focusable != null) {
					Focused.Focusable.RaiseFocusGained();
				}
			} else {
				Application.SoftKeyboard.Show(false, "");
			}
			Focused = value;
			foreach (var i in Application.Windows) {
				i.Invalidate();
			}
		}

		private BitArray GetKeysToCapture(Widget widget)
		{
			var keys = new BitArray(Key.Arrays.KeyboardKeys);
			for (var i = widget.ParentWidget; i != null; i = i.ParentWidget) {
				if (i.FocusOptions != null) {
					var t = i.FocusOptions.WantedKeys;
					t.Not();
					keys.And(t);
					t.Not();
				}
			}
			if (widget.FocusOptions != null) {
				keys.Or(widget.FocusOptions.WantedKeys);
			}
			return keys;
		}
	}

	/// <summary>
	/// Controls switching of focus between widgets based on keyboard shortcuts.
	/// </summary>
	public class KeyboardFocusSwitcher
	{
		private readonly Widget widget;

		public readonly Key FocusNext = Key.Tab;
		public readonly Key FocusPrevious = Key.MapShortcut(new Shortcut(Modifiers.Shift, Key.Tab));

		public KeyboardFocusSwitcher(Widget widget)
		{
			this.widget = widget;
			widget.Tasks.Add(FocusTask());
			var keys = (widget.FocusOptions = widget.FocusOptions ?? new FocusOptions());
			keys.WantedKeys[FocusNext] = true;
			keys.WantedKeys[FocusPrevious] = true;
		}

		private IEnumerator<object> FocusTask()
		{
			while (true) {
				if (widget.Input.WasKeyRepeated(FocusNext)) {
					AdvanceFocus(1);
				}
				if (widget.Input.WasKeyRepeated(FocusPrevious)) {
					AdvanceFocus(-1);
				}
				yield return null;
			}
		}

		private void AdvanceFocus(int direction)
		{
			var focused = KeyboardFocus.Instance.Focused;
			var focusables = GetFocusables().ToList();
			if (focused == null) {
				if (focusables.Count > 0) {
					KeyboardFocus.Instance.SetFocus(focusables[0]);
				}
			} else if (focused.DescendantOrThis(widget)) {
				var i = focusables.IndexOf(focused);
				if (i >= 0) {
					i = Mathf.Wrap(i + direction, 0, focusables.Count - 1);
					KeyboardFocus.Instance.SetFocus(focusables[i]);
				}
			}
		}

		private IEnumerable<Widget> GetFocusables()
		{
			return widget.Descendants.OfType<Widget>().Where(i => i.Focusable != null && i.Focusable.TabStop).OrderBy(i => i.Focusable.TabOrder);
		}
	}
}
