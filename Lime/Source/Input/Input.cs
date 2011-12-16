using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public enum Key
	{
		LShift = 1,
		ShiftLeft = 1,
		RShift = 2,
		ShiftRight = 2,
		LControl = 3,
		ControlLeft = 3,
		RControl = 4,
		ControlRight = 4,
		AltLeft = 5,
		LAlt = 5,
		AltRight = 6,
		RAlt = 6,
		WinLeft = 7,
		LWin = 7,
		RWin = 8,
		WinRight = 8,
		Menu = 9,
		F1 = 10,
		F2 = 11,
		F3 = 12,
		F4 = 13,
		F5 = 14,
		F6 = 15,
		F7 = 16,
		F8 = 17,
		F9 = 18,
		F10 = 19,
		F11 = 20,
		F12 = 21,
		F13 = 22,
		F14 = 23,
		F15 = 24,
		F16 = 25,
		F17 = 26,
		F18 = 27,
		F19 = 28,
		F20 = 29,
		F21 = 30,
		F22 = 31,
		F23 = 32,
		F24 = 33,
		F25 = 34,
		F26 = 35,
		F27 = 36,
		F28 = 37,
		F29 = 38,
		F30 = 39,
		F31 = 40,
		F32 = 41,
		F33 = 42,
		F34 = 43,
		F35 = 44,
		Up = 45,
		Down = 46,
		Left = 47,
		Right = 48,
		Enter = 49,
		Escape = 50,
		Space = 51,
		Tab = 52,
		Back = 53,
		BackSpace = 53,
		Insert = 54,
		Delete = 55,
		PageUp = 56,
		PageDown = 57,
		Home = 58,
		End = 59,
		CapsLock = 60,
		ScrollLock = 61,
		PrintScreen = 62,
		Pause = 63,
		NumLock = 64,
		Clear = 65,
		Sleep = 66,
		Keypad0 = 67,
		Keypad1 = 68,
		Keypad2 = 69,
		Keypad3 = 70,
		Keypad4 = 71,
		Keypad5 = 72,
		Keypad6 = 73,
		Keypad7 = 74,
		Keypad8 = 75,
		Keypad9 = 76,
		KeypadDivide = 77,
		KeypadMultiply = 78,
		KeypadMinus = 79,
		KeypadSubtract = 79,
		KeypadAdd = 80,
		KeypadPlus = 80,
		KeypadDecimal = 81,
		KeypadEnter = 82,
		A = 83,
		B = 84,
		C = 85,
		D = 86,
		E = 87,
		F = 88,
		G = 89,
		H = 90,
		I = 91,
		J = 92,
		K = 93,
		L = 94,
		M = 95,
		N = 96,
		O = 97,
		P = 98,
		Q = 99,
		R = 100,
		S = 101,
		T = 102,
		U = 103,
		V = 104,
		W = 105,
		X = 106,
		Y = 107,
		Z = 108,
		Number0 = 109,
		Number1 = 110,
		Number2 = 111,
		Number3 = 112,
		Number4 = 113,
		Number5 = 114,
		Number6 = 115,
		Number7 = 116,
		Number8 = 117,
		Number9 = 118,
		Tilde = 119,
		Minus = 120,
		Plus = 121,
		LBracket = 122,
		BracketLeft = 122,
		BracketRight = 123,
		RBracket = 123,
		Semicolon = 124,
		Quote = 125,
		Comma = 126,
		Period = 127,
		Slash = 128,
		BackSlash = 129,

		Mouse0 = 130,
		Mouse1 = 131,
		Mouse2 = 132,
		
		LastKey,
	}

	public static class Input
	{
		static bool [] previousKeysState = new bool [(int)Key.LastKey];
		static bool [] currentKeysState = new bool [(int)Key.LastKey];

		/// <summary>
		/// The matrix describes transition from pixels to virtual coordinates. 
		/// </summary>
		public static Matrix32 ScreenToWorldTransform = Matrix32.Identity;

		/// <summary>
		/// The current mouse position in virtual coordinates coordinates. (Read Only)
		/// </summary>
		public static Vector2 MousePosition { get; internal set; }

		/// <summary>
		/// Returns true while the user holds down the key identified by name. Think auto fire.
		/// </summary>
		public static bool GetKey (Key key)
		{
			return currentKeysState [(int)key];
		}

		/// <summary>
		/// Returns true during the frame the user releases the key identified by name.
		/// </summary>
		public static bool GetKeyUp (Key key)
		{
			return !currentKeysState [(int)key] && previousKeysState [(int)key];
		}

		/// <summary>
		/// Returns true during the frame the user starts pressing down the key identified by name.
		/// </summary>
		public static bool GetKeyDown (Key key)
		{
			return currentKeysState [(int)key] && !previousKeysState [(int)key];
		}

		/// <summary>
		/// After consumption, GetKeyUp (), GetKeyDown () will return false.
		/// </summary>
		public static void ConsumeKeyEvent (Key key, bool value)
		{
			if (value) {
				previousKeysState [(int)key] = currentKeysState [(int)key];
			}
		}

#if WIN
		internal static void Update ()
		{
			currentKeysState.CopyTo (previousKeysState, 0);
			var kbdState = OpenTK.Input.Keyboard.GetState (0);
			for (int i = (int)Key.LShift; i <= (int)Key.BackSlash; i++) {
				currentKeysState [i] = kbdState.IsKeyDown ((OpenTK.Input.Key)i);
			}
			var mouseState = OpenTK.Input.Mouse.GetState (0);
			currentKeysState [(int)Key.Mouse0] = mouseState.IsButtonDown (OpenTK.Input.MouseButton.Left);
			currentKeysState [(int)Key.Mouse1] = mouseState.IsButtonDown (OpenTK.Input.MouseButton.Right);
			currentKeysState [(int)Key.Mouse2] = mouseState.IsButtonDown (OpenTK.Input.MouseButton.Middle);
		}
#endif
	}
}
