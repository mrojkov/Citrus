using System;
using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// The WidgetInput class allows a widget to capture an input device (mouse, keyboard).
	/// After capturing the device, the widget and all its children receive an actual buttons and device axes state (e.g. mouse position). Other widgets receive released buttons state and frozen axes values.
	/// </summary>
	public class WidgetInput
	{
		private Widget widget;
		private Vector2 lastMousePosition;
		private Vector2[] lastTouchPositions;
		private static List<CaptureStackItem> captureStack;
		private static bool skipCapturesCleanup;
			
		public static IEnumerable<CaptureStackItem> CaptureStack { get { return captureStack; } }

		static WidgetInput()
		{
			captureStack = new List<CaptureStackItem>();
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

		public string TextInput 
		{
			get { return IsAcceptingKeyboard() ? WindowInput.TextInput : string.Empty; }
		}

		public Vector2 MousePosition
		{
			get
			{
				if (IsAcceptingKey(Key.Mouse0)) {
					lastMousePosition = WindowInput.MousePosition;
				}
				return lastMousePosition;
			}
		}

		public Vector2 GetTouchPosition(int index)
		{
			if (IsAcceptingKey(Key.Touch0)) {
				lastTouchPositions[index] = WindowInput.GetTouchPosition(index);
			}
			return lastTouchPositions[index];
		}

		public int GetNumTouches()
		{
			return IsAcceptingKey(Key.Touch0) ? WindowInput.GetNumTouches() : 0;
		}

		public void CaptureMouse()
		{
			Capture(KeySets.Mouse);
		}

		/// <summary>
		/// Captures the mouse only for the given widget.
		/// </summary>
		public void CaptureMouseExclusive()
		{
			CaptureExclusive(KeySets.Mouse);
		}

		public void CaptureKeyboard()
		{
			Capture(KeySets.Keyboard);
		}

		/// <summary>
		/// Captures the keyboard only for the given widget.
		/// </summary>
		public void CaptureKeyboardExclusive()
		{
			CaptureExclusive(KeySets.Keyboard);
		}

		public void Capture(BitSet256 keys)
		{
			CaptureHelper(keys, false);
		}

		public void CaptureExclusive(BitSet256 keys)
		{
			CaptureHelper(keys, true);
		}

		private void CaptureHelper(BitSet256 keys, bool exclusive)
		{
			var thisLayer = widget.GetEffectiveLayer();
			var t = captureStack.FindLastIndex(i => i.Widget.GetEffectiveLayer() <= thisLayer);
#if DEBUG
			captureStack.Insert(t + 1, new CaptureStackItem { Widget = widget, Keys = keys, StackTrace = System.Environment.StackTrace, Exclusive = exclusive });
#else
			stack.Insert(t + 1, new StackItem { Widget = widget, Keys = keys, Exclusive = exclusive  });
#endif
			// The widget may be invisible right after creation, 
			// so omit the stack cleaning up on this frame.
			skipCapturesCleanup = true;
		}

		public void Release()
		{
			for (int i = captureStack.Count - 1; i >= 0; i--) {
				if (captureStack[i].Widget == widget) {
					captureStack.RemoveAt(i);
					break;
				}
			}
		}

		public bool IsMouseOwner()
		{
			return IsKeyOwner(Key.Mouse0);
		}

		public bool IsKeyboardOwner()
		{
			return IsKeyOwner(Key.A);
		}

		public bool IsKeyOwner(Key key)
		{
			for (int i = captureStack.Count - 1; i >= 0; i--) {
				var t = captureStack[i];
				if (t.Keys[key.Code]) {
					return t.Widget == widget;
				}
			}
			return false;
		}

		public bool IsAcceptingMouse()
		{
			return IsAcceptingKey(Key.Mouse0);
		}

		public bool IsAcceptingKeyboard()
		{
			return IsAcceptingKey(Key.A);
		}

		public bool IsAcceptingKey(Key key)
		{
			for (int i = captureStack.Count - 1; i >= 0; i--) {
				var t = captureStack[i];
				if (t.Keys[key.Code]) {
					return t.Widget == widget || (!t.Exclusive && widget.DescendantOf(t.Widget));
				}
			}
			return true;				
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
			return WindowInput.IsKeyPressed(key) && IsAcceptingKey(key);
		}

		public bool WasKeyPressed(Key key)
		{
			return WindowInput.WasKeyPressed(key) && IsAcceptingKey(key);
		}

		public bool WasKeyReleased(Key key)
		{
			return WindowInput.WasKeyReleased(key) && IsAcceptingKey(key);
		}

		public bool WasKeyRepeated(Key key)
		{
			return WindowInput.WasKeyRepeated(key) && IsAcceptingKey(key);
		}

		public void CaptureAll()
		{
			Capture(BitSet256.Full);
		}

		public void CaptureAllExclusive()
		{
			CaptureExclusive(BitSet256.Full);
		}

		internal static void RemoveInvalidatedCaptures()
		{
			if (!skipCapturesCleanup) {
				captureStack.RemoveAll(i => !IsWidgetShown(i.Widget));
			}
			skipCapturesCleanup = false;
		}

		private static bool IsWidgetShown(Widget widget)
		{
			return WidgetContext.Current.Root == widget.GetRoot() && widget.GloballyVisible;
		}

		private Input WindowInput
		{
			get { return CommonWindow.Current.Input; }
		}

		public class CaptureStackItem
		{
			public Widget Widget;
			public BitSet256 Keys;
#if DEBUG
			public string StackTrace;
#endif
			public bool Exclusive;
		}
	}
}
