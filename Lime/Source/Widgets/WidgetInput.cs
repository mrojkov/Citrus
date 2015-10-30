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
				lastMousePosition = Input.MousePosition;
			}
			return lastMousePosition;
		}

		public Vector2 GetTouchPosition(int index)
		{
			if (IsAcceptingTouchScreen()) {
				lastTouchPositions[index] = Input.GetTouchPosition(index);
			}
			return lastTouchPositions[index];
		}

		public int GetNumTouches()
		{
			return IsAcceptingTouchScreen() ? Input.GetNumTouches() : 0;
		}

		public void CaptureMouse()
		{
			Capture(mouseCaptureStack);
		}

		private void Capture(List<StackItem> stack)
		{
			stack.RemoveAll(i => i.Widget == widget);
			var thisLayer = widget.GetEffectiveLayer();
			var t = stack.FindLastIndex(i => i.Widget.GetEffectiveLayer() <= thisLayer);
#if DEBUG
			stack.Insert(t + 1, new StackItem() { Widget = widget, StackTrace = System.Environment.StackTrace });
#else
			stack.Insert(t + 1, new StackItem() { Widget = widget });
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

		public void CaptureTouchScreen()
		{
			CaptureMouse();
		}

		public void ReleaseTouchScreen()
		{
			ReleaseMouse();
		}

		public bool IsTouchScreenOwner()
		{
			return IsMouseOwner();
		}

		public void CaptureKeyboard()
		{
			Capture(keyboardCaptureStack);
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
				return IsAcceptingKeyboard() ? Input.TextInput : string.Empty;
			}
		}

		public bool IsMousePressed(int button = 0)
		{
			return IsAcceptingMouse() && Input.IsMousePressed(button);
		}

		public bool WasMousePressed(int button = 0)
		{
			return IsAcceptingMouse() && Input.WasMousePressed(button);
		}

		public bool WasMouseReleased(int button = 0)
		{
			return IsAcceptingMouse() && Input.WasMouseReleased(button);
		}

		public float WheelScrollAmount
		{
			get { return IsAcceptingMouse() ? Input.WheelScrollAmount : 0; } 
		}

		public bool IsKeyPressed(Key key)
		{
			return IsAcceptingDeviceWithKey(key) && Input.IsKeyPressed(key);
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
				if (Input.IsKeyPressed(k) != (k == key))
					return false;
			}
			return true;
		}

		public bool WasKeyPressed(Key key)
		{
			return IsAcceptingDeviceWithKey(key) && Input.WasKeyPressed(key);
		}

		public bool WasKeyReleased(Key key)
		{
			return IsAcceptingDeviceWithKey(key) && Input.WasKeyReleased(key);
		}

		private bool IsAcceptingDeviceWithKey(Key key)
		{
			switch (key) {
				case Key.Mouse0:
				case Key.Mouse1:
				case Key.Mouse2:
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
			return context.Widget == widget || widget.ChildOf(context.Widget);
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
			return context.Widget == widget || widget.ChildOf(context.Widget);
		}

		public void CaptureAll()
		{
			CaptureMouse();
			CaptureKeyboard();
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
			return (widget.GetRoot() == World.Instance) && widget.GloballyVisible;
		}
	}
}
