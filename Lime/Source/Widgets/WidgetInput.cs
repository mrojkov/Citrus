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
		private Widget widget;
		private Vector2 lastMousePosition;
		private Vector2[] lastTouchPositions;
		// NB: TouchScreen and mouse share the same capture stack
		static List<Widget> mouseCaptureStack;
		static List<Widget> keyboardCaptureStack;

		static WidgetInput()
		{
			mouseCaptureStack = new List<Widget>();
			keyboardCaptureStack = new List<Widget>();
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
			mouseCaptureStack.Remove(widget);
			mouseCaptureStack.Add(widget);
		}

		public void ReleaseMouse()
		{
			mouseCaptureStack.Remove(widget);
		}

		public bool IsMouseOwner()
		{
			int c = mouseCaptureStack.Count;
			return (c > 0) && mouseCaptureStack[c - 1] == widget;
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
			keyboardCaptureStack.Remove(widget);
			keyboardCaptureStack.Add(widget);
		}

		public void ReleaseKeyboard()
		{
			keyboardCaptureStack.Remove(widget);
		}

		public bool IsKeyboardOwner()
		{
			int c = keyboardCaptureStack.Count;
			return (c > 0) && keyboardCaptureStack[c - 1] == widget;
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

		public bool IsKeyPressed(Key key)
		{
			return IsAcceptingDeviceWithKey(key) && Input.IsKeyPressed(key);
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
			return context == widget || widget.ChildOf(context);
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
			return context == widget || widget.ChildOf(context);
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

		internal static void RemoveInvalidatedCaptivities()
		{
			keyboardCaptureStack.RemoveAll(i => !IsVisibleWidget(i));
			mouseCaptureStack.RemoveAll(i => !IsVisibleWidget(i));
		}

		private static bool IsVisibleWidget(Widget widget)
		{
			return (widget.GetRoot() == World.Instance) && widget.GloballyVisible;
		}
	}
}
