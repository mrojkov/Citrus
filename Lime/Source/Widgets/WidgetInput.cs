using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// The WidgetInput class allows a widget to capture an input device (mouse, keyboard, touchscreen).
	/// After capturing the device, the widget and all its children receive an actual buttons and device axes state (e.g. mouse position). Other widgets receive released buttons state and frozen axes values.
	/// </summary>
	public class WidgetInput
	{
		public struct StackItem
		{
			public Widget Widget;
#if DEBUG
			public string StackTrace;
#endif
			public bool Exclusive;
		}

		private Input WindowInput
		{
			get { return Window.Current.Input; }
		}

		private Widget widget;
		private Vector2 lastMousePosition;
		private Vector2[] lastTouchPositions;
		// NB: TouchScreen and mouse share the same capture stack
		static List<StackItem> mouseCaptureStack;
		static List<StackItem> keyboardCaptureStack;
		static bool skipCapturesCleanup;

		public static IEnumerable<StackItem> MouseCaptureStack { get { return mouseCaptureStack; } }
		public static IEnumerable<StackItem> KeyboardCaptureStack { get { return keyboardCaptureStack; } }
		
		static WidgetInput()
		{
			mouseCaptureStack = new List<StackItem>();
			keyboardCaptureStack = new List<StackItem>();
		}

		public WidgetInput(Widget widget)
		{
			this.widget = widget;
			lastMousePosition = Vector2.PositiveInfinity;
			lastTouchPositions = new Vector2[Input.MaxTouches];
			for (int i = 0; i < Input.MaxTouches; i++) {
				lastTouchPositions[i] = Vector2.PositiveInfinity;
			}
		}

		public Vector2 MousePosition
		{
			get { return GetMousePosition(); }
		}

		private Vector2 GetMousePosition()
		{
			if (IsAcceptingMouse()) {
				lastMousePosition = WindowInput.MousePosition;
			}
			return lastMousePosition;
		}

		public Vector2 GetTouchPosition(int index)
		{
			if (IsAcceptingTouchScreen()) {
				lastTouchPositions[index] = WindowInput.GetTouchPosition(index);
			}
			return lastTouchPositions[index];
		}

		public int GetNumTouches()
		{
			return IsAcceptingTouchScreen() ? WindowInput.GetNumTouches() : 0;
		}

		public void CaptureMouse()
		{
			Capture(mouseCaptureStack, false);
		}

		/// <summary>
		/// Captures the mouse only for the given widget.
		/// </summary>
		public void CaptureMouseExclusive()
		{
			Capture(mouseCaptureStack, true);
		}

		private void Capture(List<StackItem> stack, bool exclusive)
		{
			stack.RemoveAll(i => i.Widget == widget);
			var thisLayer = widget.GetEffectiveLayer();
			var t = stack.FindLastIndex(i => i.Widget.GetEffectiveLayer() <= thisLayer);
#if DEBUG
			stack.Insert(t + 1, new StackItem() { Widget = widget, StackTrace = System.Environment.StackTrace, Exclusive = exclusive });
#else
			stack.Insert(t + 1, new StackItem() { Widget = widget, Exclusive = exclusive  });
#endif
			// The widget may be invisible right after creation, 
			// so omit the stack cleaning up on this frame.
			skipCapturesCleanup = true;
		}

		public void ReleaseMouse()
		{
			mouseCaptureStack.RemoveAll(i => i.Widget == widget);
		}

		public bool IsMouseOwner()
		{
			int c = mouseCaptureStack.Count;
			return (c > 0) && mouseCaptureStack[c - 1].Widget == widget;
		}

		public bool IsTouchScreenOwner()
		{
			return IsMouseOwner();
		}

		public void CaptureKeyboard()
		{
			Capture(keyboardCaptureStack, false);
		}

		/// <summary>
		/// Captures the keyboard only for the given widget.
		/// </summary>
		public void CaptureKeyboardExclusive()
		{
			Capture(keyboardCaptureStack, true);
		}

		public void ReleaseKeyboard()
		{
			keyboardCaptureStack.RemoveAll(i => i.Widget == widget);
		}

		public bool IsKeyboardOwner()
		{
			int c = keyboardCaptureStack.Count;
			return (c > 0) && keyboardCaptureStack[c - 1].Widget == widget;
		}

		public string TextInput { 
			get {
				return IsAcceptingKeyboard() ? WindowInput.TextInput : string.Empty;
			}
		}

		public bool IsMousePressed(int button = 0)
		{
			return WindowInput.IsMousePressed(button) && IsAcceptingMouse();
		}

		public bool WasMousePressed(int button = 0)
		{
			return WindowInput.WasMousePressed(button) && IsAcceptingMouse();
		}

		public bool WasMouseReleased(int button = 0)
		{
			return WindowInput.WasMouseReleased(button) && IsAcceptingMouse();
		}

		public float WheelScrollAmount
		{
			get { return IsAcceptingMouse() ? WindowInput.WheelScrollAmount : 0; } 
		}

		public bool IsKeyPressed(Key key)
		{
			return WindowInput.IsKeyPressed(key) && IsAcceptingDeviceWithKey(key);
		}

		/// <summary>
		/// Returns true if only a single given key from the given range is pressed.
		/// Useful for recognizing keyboard modifiers.
		/// </summary>
		public bool IsSingleKeyPressed(Key key, Key rangeMin, Key rangeMax)
		{
			if (!IsAcceptingDeviceWithKey(key))
				return false;
			for (var k = rangeMin; k <= rangeMax; ++k) {
				if (WindowInput.IsKeyPressed(k) != (k == key))
					return false;
			}
			return true;
		}

		public bool WasKeyPressed(Key key)
		{
			return WindowInput.WasKeyPressed(key) && IsAcceptingDeviceWithKey(key);
		}

		public bool WasKeyReleased(Key key)
		{
			return WindowInput.WasKeyReleased(key) && IsAcceptingDeviceWithKey(key);
		}

		public bool WasKeyRepeated(Key key)
		{
			return WindowInput.WasKeyRepeated(key) && IsAcceptingDeviceWithKey(key);
		}

		private bool IsAcceptingDeviceWithKey(Key key)
		{
			switch (key) {
				case Key.Mouse0:
				case Key.Mouse1:
				case Key.Mouse2:
				case Key.Mouse0DoubleClick:
				case Key.Mouse1DoubleClick:
					return IsAcceptingMouse();
				case Key.Touch0:
				case Key.Touch1:
				case Key.Touch2:
				case Key.Touch3:
					return IsAcceptingTouchScreen();
				default:
					return IsAcceptingKeyboard();
			}
		}

		public bool IsAcceptingMouse()
		{
			int c = mouseCaptureStack.Count;
			if (c == 0) {
				return true;
			}
			var context = mouseCaptureStack[c - 1];
			return context.Widget == widget || (!context.Exclusive && widget.ChildOf(context.Widget));
		}

		public bool IsAcceptingTouchScreen()
		{
			return IsAcceptingMouse();
		}

		public bool IsAcceptingKeyboard()
		{
			int c = keyboardCaptureStack.Count;
			if (c == 0) {
				return true;
			}
			var context = keyboardCaptureStack[c - 1];
			return context.Widget == widget || (!context.Exclusive && widget.ChildOf(context.Widget));
		}

		public void CaptureAll()
		{
			CaptureMouse();
			CaptureKeyboard();
		}

		public void CaptureAllExclusive()
		{
			CaptureMouseExclusive();
			CaptureKeyboardExclusive();
		}

		public void ReleaseAll()
		{
			ReleaseMouse();
			ReleaseKeyboard();
		}

		internal static void RemoveInvalidatedCaptures()
		{
			if (!skipCapturesCleanup) {
				keyboardCaptureStack.RemoveAll(i => !IsVisibleWidget(i.Widget));
				mouseCaptureStack.RemoveAll(i => !IsVisibleWidget(i.Widget));
			}
			skipCapturesCleanup = false;
		}

		private static bool IsVisibleWidget(Widget widget)
		{
			return WidgetContext.Current.Root == widget.GetRoot() && widget.GloballyVisible;
		}
	}
}
