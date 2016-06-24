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
		private Vector2 lastMousePosition;
		private Vector2[] lastTouchPositions;
		private static List<CaptureStackItem> captureStack;
			
		public Input WindowInput { get { return CommonWindow.Current.Input; } }
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

		public void Dispose()
		{
			captureStack.RemoveAll(i => i.Widget == widget);
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
			Capture(Key.Arrays.MouseButtons);
		}

		/// <summary>
		/// Captures the mouse only for the given widget.
		/// </summary>
		public void CaptureMouseExclusive()
		{
			CaptureExclusive(Key.Arrays.MouseButtons);
		}

		public void CaptureKeyboard()
		{
			Capture(Key.Arrays.KeyboardKeys);
		}

		/// <summary>
		/// Captures the keyboard only for the given widget.
		/// </summary>
		public void CaptureKeyboardExclusive()
		{
			CaptureExclusive(Key.Arrays.KeyboardKeys);
		}

		public void CaptureAll()
		{
			Capture(Key.Arrays.AllKeys);
		}

		public void CaptureAllExclusive()
		{
			CaptureExclusive(Key.Arrays.AllKeys);
		}

		public void Capture(BitArray keys)
		{
			CaptureHelper(keys, false);
		}

		public void CaptureExclusive(BitArray keys)
		{
			CaptureHelper(keys, true);
		}

		private void CaptureHelper(BitArray keys, bool exclusive)
		{
			var thisLayer = widget.GetEffectiveLayer();
			var t = captureStack.FindLastIndex(i => i.Widget.GetEffectiveLayer() <= thisLayer);
#if DEBUG
			captureStack.Insert(t + 1, new CaptureStackItem { Widget = widget, Keys = keys, StackTrace = System.Environment.StackTrace, Exclusive = exclusive });
#else
			captureStack.Insert(t + 1, new CaptureStackItem { Widget = widget, Keys = keys, Exclusive = exclusive  });
#endif
		}

		public void ReleaseMouse()
		{
			Release(Key.Arrays.MouseButtons);
		}

		public void ReleaseKeyboard()
		{
			Release(Key.Arrays.KeyboardKeys);
		}

		public void ReleaseAll()
		{
			Release(Key.Arrays.AllKeys);
		}

		public void Release(BitArray keys)
		{
			for (int i = captureStack.Count - 1; i >= 0; i--) {
				if (captureStack[i].Widget == widget && BitArraysEqual(captureStack[i].Keys, keys)) {
					captureStack.RemoveAt(i);
					break;
				}
			}
		}

		private bool BitArraysEqual(BitArray lhs, BitArray rhs)
		{
			if (lhs.Count != rhs.Count) {
				return false;
			}
			for (int i = 0; i < lhs.Count; i++) {
				if (lhs.Get(i) != rhs.Get(i)) {
					return false;
				}
			}
			return true;
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

		public bool IsKeyEnabled(Key key)
		{
			return WindowInput.IsKeyEnabled(key);
		}
		
		public void EnableKey(Key key, bool enable)
		{
			if (IsAcceptingKey(key)) {
				WindowInput.EnableKey(key, enable);
			}
		}

		public class CaptureStackItem
		{
			public Widget Widget;
			public BitArray Keys;
#if DEBUG
			public string StackTrace;
#endif
			public bool Exclusive;
		}
	}
}
