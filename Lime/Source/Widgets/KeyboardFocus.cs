using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Controls whether a widget could be traversed with Tab or Shift+Tab.
	/// </summary>
	public class TabTraversable
	{
		public int Order { get; set; }
	}

	/// <summary>
	/// Indicates whether all of widget's descendants should be within the one tab-traverse scope. 
	/// The only way to change the current scope, is to activate a widget inside any other tab-traverse scope with the mouse.
	/// </summary>
	public class TabTraverseScope
	{
	}

	/// <summary>
	/// Defines a set of keys which would be captured by a widget. By default the focused widget accepts all keyboard keys, except keys wanted one of it ancestors.
	/// </summary>
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
			if (Focused != null && Focused.DescendantOrThis(WidgetContext.Current.Root)) {
				focusPerWindow[window] = Focused;
			} else if (Focused == null) {
				focusPerWindow[window] = null;
			}
		}

		private void Window_Activated(IWindow window)
		{
			if (Focused != null && Focused.DescendantOrThis(WidgetContext.Current.Root)) {
				return;
			}
			Widget newFocused;
			if (focusPerWindow.TryGetValue(window, out newFocused)) {
				if (newFocused != null && newFocused.DescendantOrThis(WidgetContext.Current.Root)) {
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
			}
			if (value != null) {
				capturedKeys = GetKeysToCapture(value);
				value.Input.Capture(capturedKeys);
				Application.SoftKeyboard.Show(true, value.Text);
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
	/// Controls switching of focus between widgets with Tab or Shift+Tab.
	/// </summary>
	public class TabTraverseController
	{
		private readonly Widget widget;
		private Widget lastFocused;

		public readonly Key FocusNext = Key.Tab;
		public readonly Key FocusPrevious = Key.MapShortcut(new Shortcut(Modifiers.Shift, Key.Tab));

		public TabTraverseController(Widget widget)
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
				var focused = KeyboardFocus.Instance.Focused;
				if (focused != null && focused.DescendantOrThis(widget)) {
					lastFocused = focused;
				}
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
			if (!CanRegainFocus()) {
				KeyboardFocus.Instance.SetFocus(GetTabTraversables(widget).FirstOrDefault());
			} else if (KeyboardFocus.Instance.Focused == null) {
				lastFocused.SetFocus();
			} else {
				var scope = GetTabTraverseScope(lastFocused);
				var traversables = GetTabTraversables(scope).ToList();
				var i = traversables.IndexOf(lastFocused);
				if (i >= 0) {
					i = Mathf.Wrap(i + direction, 0, traversables.Count - 1);
					traversables[i].SetFocus();
				}
			}
		}

		private bool CanRegainFocus()
		{
			return lastFocused != null && lastFocused.GloballyVisible && lastFocused.DescendantOrThis(WidgetContext.Current.Root);
		}

		private Widget GetTabTraverseScope(Widget widget)
		{
			for (; widget.Parent != null; widget = widget.ParentWidget) {
				if (widget.TabTraverseScope != null) {
					break;
				}
			}
			return widget;
		}

		private IEnumerable<Widget> GetTabTraversables(Widget root)
		{
			return root.Descendants.OfType<Widget>().Where(i => i.TabTravesable != null && i.GloballyVisible).OrderBy(i => i.TabTravesable.Order);
		}
	}
}
