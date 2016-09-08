using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// The WidgetInput class allows a widget to capture an input device (mouse, keyboard).
	/// After capturing the device, the widget and all its children receive an actual buttons and device axes state (e.g. mouse position). Other widgets receive released buttons state and frozen axes values.
	/// </summary>
	public class WidgetInput
	{
		public delegate void KeyEventHandler(WidgetInput input, Key key);

		private Widget widget;
		private Input windowInput { get { return CommonWindow.Current.Input; } }
		private WidgetContext context { get { return WidgetContext.Current; } }

		public event KeyEventHandler KeyPressed;
		public event KeyEventHandler KeyReleased;
		public event KeyEventHandler KeyRepeated;

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
			MouseCaptureStack.Instance.CaptureMouse(widget);
		}

		public void ReleaseMouse()
		{
			MouseCaptureStack.Instance.ReleaseMouse(widget);
		}

		public bool IsMouseOwner() { return widget == MouseCaptureStack.Instance.MouseOwner; }

		public bool IsAcceptingMouse()
		{
			return IsAcceptingKey(Key.Mouse0);
		}

		public bool IsAcceptingKey(Key key)
		{
			if (key.IsMouseButton()) {
				var mouseOwner = MouseCaptureStack.Instance.MouseOwner;
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
						widget.HitTestTarget && widget.IsInsideBoundingRect(MousePosition);
				} else {
					return nodeUnderMouse == widget;
				}
			} else {
				var focused = Widget.Focused;
				return focused != null && focused.DescendantOrThis(widget);
			}
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

		public bool IsKeyEnabled(Key key)
		{
			return windowInput.IsKeyEnabled(key);
		}
		
		public void EnableKey(Key key, bool enable)
		{
			if (IsAcceptingKey(key)) {
				windowInput.EnableKey(key, enable);
			}
		}

		public void ConsumeKey(Key key)
		{
			if (IsAcceptingKey(key)) {
				windowInput.ConsumeKey(key);
			}
		}

		internal void DispatchEvents()
		{
			if (!windowInput.Changed) {
				return;
			}
			if (KeyPressed != null) {
				foreach (var key in Key.Enumerate()) {
					if (WasKeyPressed(key)) {
						KeyPressed(this, key);
					}
				}
			}
			if (KeyRepeated != null) {
				foreach (var key in Key.Enumerate()) {
					if (WasKeyRepeated(key)) {
						KeyRepeated(this, key);
					}
				}
			}
			if (KeyReleased != null) {
				foreach (var key in Key.Enumerate()) {
					if (WasKeyReleased(key)) {
						KeyReleased(this, key);
					}
				}
			}
		}
	}

	public class MouseCaptureStack
	{
		public static readonly MouseCaptureStack Instance = new MouseCaptureStack();

		readonly List<Widget> stack = new List<Widget>();

		public Widget MouseOwner { get; private set; }

		private MouseCaptureStack() { }

		public void CaptureMouse(Widget widget)
		{
			var thisLayer = widget.GetEffectiveLayer();
			var t = stack.FindLastIndex(i => i.GetEffectiveLayer() <= thisLayer);
			stack.Insert(t + 1, widget);
			RefreshMouseOwner();
		}

		public void ReleaseMouse(Widget widget)
		{
			var i = stack.IndexOf(widget);
			if (i >= 0) {
				stack.RemoveAt(i);
			}
			RefreshMouseOwner();
		}

		private void RefreshMouseOwner()
		{
			int i = stack.Count;
			MouseOwner = i > 0 ? stack[i - 1] : null;
		}
	}
}
