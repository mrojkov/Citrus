using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// The WidgetInput class allows a widget to capture an input device (mouse, keyboard).
	/// After capturing the device, the widget and all its children receive an actual buttons and device axes state (e.g. mouse position). Other widgets receive released buttons state and frozen axes values.
	/// </summary>
	public class WidgetInput : IDisposable
	{
		private Widget widget;
		private Input windowInput { get { return CommonWindow.Current.Input; } }
		private WidgetContext context { get { return WidgetContext.Current; } }

		public static readonly WidgetStack MouseCaptureStack = new WidgetStack();
		public static readonly WidgetStack InputScopeStack = new WidgetStack();

		public delegate bool FilterFunc(Widget widget, Key key);
		public static FilterFunc Filter;

		public static bool AcceptMouseBeyondWidgetByDefault = true;

		/// <summary>
		/// Indicates whether mouse events should be accepted even the widget is not under the mouse cursor.
		/// </summary>
		public bool AcceptMouseBeyondWidget = AcceptMouseBeyondWidgetByDefault;

		/// <summary>
		/// Indicates whether mouse events should be accepted even the mouse is over one of widget's descendant.
		/// </summary>
		public bool AcceptMouseThroughDescendants;

		public WidgetInput(Widget widget)
		{
			this.widget = widget;
		}

		public string TextInput
		{
			get { return widget.IsFocused() ? windowInput.TextInput : string.Empty; }
		}

		public Vector2 MousePosition { get { return windowInput.MousePosition; } }

		public Vector2 LocalMousePosition { get { return windowInput.MousePosition * widget.LocalToWorldTransform.CalcInversed(); } }

		public Vector2 GetTouchPosition(int index)
		{
			return windowInput.GetTouchPosition(index);
		}

		public int GetNumTouches()
		{
			return IsAcceptingKey(Key.Touch0) ? windowInput.GetNumTouches() : 0;
		}

		public void CaptureMouse()
		{
			MouseCaptureStack.Add(widget);
		}

		public void ReleaseMouse()
		{
			MouseCaptureStack.Remove(widget);
		}

		public bool IsMouseOwner() { return widget == MouseCaptureStack.Top; }

		public bool IsAcceptingMouse()
		{
			return IsAcceptingKey(Key.Mouse0);
		}

		public bool IsAcceptingKey(Key key)
		{
			if (Filter != null && !Filter(widget, key)) {
				return false;
			}
			if (InputScopeStack.Top != null && !widget.DescendantOrThis(InputScopeStack.Top)) {
				return false;
			}
			if (key.IsMouseKey()) {
				var mouseOwner = MouseCaptureStack.Top;
				if (mouseOwner != null) {
					return mouseOwner == widget;
				}
				if (AcceptMouseBeyondWidget) {
					return true;
				}
				var nodeUnderMouse = WidgetContext.Current.NodeUnderMouse;
				if (AcceptMouseThroughDescendants) {
					return
						nodeUnderMouse != null && nodeUnderMouse.DescendantOrThis(widget) &&
						// Full HitTest would be better.
						widget.HitTestTarget && widget.BoundingRectHitTest(MousePosition);
				}
				return nodeUnderMouse == widget;
			}
			if (key.IsModifier()) {
				return true;
			}
			var focused = Widget.Focused;
			return focused != null && focused.DescendantOrThis(widget);
		}

		public bool IsMousePressed(int button = 0)
		{
			return IsKeyPressed(Input.GetMouseButtonByIndex(button));
		}

		public bool WasMousePressed(int button = 0)
		{
			return WasKeyPressed(Input.GetMouseButtonByIndex(button));
		}

		public bool WasMouseReleased(int button = 0)
		{
			return WasKeyReleased(Input.GetMouseButtonByIndex(button));
		}

		public float WheelScrollAmount
		{
			get { return IsAcceptingKey(Key.MouseWheelUp) ? windowInput.WheelScrollAmount : 0; }
		}

		public bool IsKeyPressed(Key key)
		{
			return windowInput.IsKeyPressed(key) && IsAcceptingKey(key);
		}

		public bool WasKeyPressed(Key key)
		{
			return windowInput.WasKeyPressed(key) && IsAcceptingKey(key);
		}

		public bool ConsumeKeyPress(Key key)
		{
			if (WasKeyPressed(key)) {
				ConsumeKey(key);
				return true;
			}
			return false;
		}

		public bool WasKeyReleased(Key key)
		{
			return windowInput.WasKeyReleased(key) && IsAcceptingKey(key);
		}

		public bool ConsumeKeyRelease(Key key)
		{
			if (WasKeyReleased(key)) {
				ConsumeKey(key);
				return true;
			}
			return false;
		}

		public bool WasKeyRepeated(Key key)
		{
			return windowInput.WasKeyRepeated(key) && IsAcceptingKey(key);
		}

		public bool ConsumeKeyRepeat(Key key)
		{
			if (WasKeyRepeated(key)) {
				ConsumeKey(key);
				return true;
			}
			return false;
		}

		public void ConsumeKey(Key key)
		{
			if (IsAcceptingKey(key)) {
				windowInput.ConsumeKey(key);
			}
		}

		public void ConsumeKeys(List<Key> keys)
		{
			foreach (var key in keys) {
				ConsumeKey(key);
			}
		}

		public void ConsumeKeys(IEnumerable<Key> keys)
		{
			foreach (var key in keys) {
				ConsumeKey(key);
			}
		}

		/// <summary>
		/// Restricts input scope with the current widget and its descendants.
		/// </summary>
		public void RestrictScope()
		{
			InputScopeStack.Add(widget);
		}

		/// <summary>
		/// Derestricts input scope from the current widget and its descendants.
		/// </summary>
		public void DerestrictScope()
		{
			InputScopeStack.Remove(widget);
		}

		[Obsolete("Use RestrictScope() instead")]
		public void CaptureAll()
		{
			RestrictScope();
		}

		[Obsolete("Use DerestrictScope() instead")]
		public void ReleaseAll()
		{
			DerestrictScope();
		}

		public class WidgetStack : IReadOnlyList<Widget>
		{
			private readonly List<Widget> stack = new List<Widget>();

			public Widget Top { get; private set; }

			public void Add(Widget widget)
			{
				var thisLayer = widget.GetEffectiveLayer();
				var t = stack.FindLastIndex(i => i.GetEffectiveLayer() <= thisLayer);
				stack.Insert(t + 1, widget);
				RefreshTop();
			}

			public void Remove(Widget widget)
			{
				var i = stack.IndexOf(widget);
				if (i >= 0) {
					stack.RemoveAt(i);
				}
				RefreshTop();
			}

			public void RemoveAll(Predicate<Widget> match)
			{
				stack.RemoveAll(match);
				RefreshTop();
			}

			private void RefreshTop()
			{
				int i = stack.Count;
				Top = i > 0 ? stack[i - 1] : null;
			}

			public IEnumerator<Widget> GetEnumerator()
			{
				return stack.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable) stack).GetEnumerator();
			}

			public int Count
			{
				get { return stack.Count; }
			}

			public Widget this[int index]
			{
				get { throw new NotImplementedException(); }
			}
		}

		public void Dispose()
		{
			InputScopeStack.RemoveAll(i => i == widget);
			MouseCaptureStack.RemoveAll(i => i == widget);
		}
	}
}
