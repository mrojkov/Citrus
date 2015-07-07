#if MAC
using System;
using AppKit;

namespace OpenTK.Input
{
	[Flags]
	internal enum CocoaKeyModifiers
	{
		None     = 0,
		Shift    = 0x0200,
		CapsLock = 0x0400,
		Control  = 0x1000,
		Command  = 0x0100,
		Option   = 0x0800,
	}

	internal enum CocoaKeyCode
	{
		A = 0,
		B = 11,
		C = 8,
		D = 2,
		E = 14,
		F = 3,
		G = 5,
		H = 4,
		I = 34,
		J = 38,
		K = 40,
		L = 37,
		M = 46,
		N = 45,
		O = 31,
		P = 35,
		Q = 12,
		R = 15,
		S = 1,
		T = 17,
		U = 32,
		V = 9,
		W = 13,
		X = 7,
		Y = 16,
		Z = 6,

		Key_1 = 18,
		Key_2 = 19,
		Key_3 = 20,
		Key_4 = 21,
		Key_5 = 23,
		Key_6 = 22,
		Key_7 = 26,
		Key_8 = 28,
		Key_9 = 25,
		Key_0 = 29,

		Space = 49,
		Tilde = 50,

		Minus = 27,
		Equals = 24,
		BracketLeft = 33,
		BracketRight = 30,
		Backslash = 42,
		Semicolon = 41,
		Quote = 39,
		Comma = 43,
		Period = 47,
		Slash = 44,

		Enter = 36,
		Tab = 48,
		Backspace = 51,
		Return = 52,
		Esc = 53,

		Command = 55,
		Shift = 56,
		CapsLock = 57,
		OptionAlt = 58,
		Control = 59,

		KeyPad_Decimal = 65,
		KeyPad_Multiply = 67,
		KeyPad_Add = 69,
		KeyPad_Divide = 75,
		KeyPad_Enter = 76,
		KeyPad_Subtract = 78,
		KeyPad_Equal = 81,
		KeyPad_0 = 82,
		KeyPad_1 = 83,
		KeyPad_2 = 84,
		KeyPad_3 = 85,
		KeyPad_4 = 86,
		KeyPad_5 = 87,
		KeyPad_6 = 88,
		KeyPad_7 = 89,
		KeyPad_8 = 91,
		KeyPad_9 = 92,

		F1 = 122,
		F2 = 120,
		F3 = 99,
		F4 = 118,
		F5 = 96,
		F6 = 97,
		F7 = 98,
		F8 = 100,
		F9 = 101,
		F10 = 109,
		F11 = 103,
		F12 = 111,
		F13 = 105,
		F14 = 107,
		F15 = 113,

		Menu = 110,

		Insert = 114,
		Home = 115,
		Pageup = 116,
		Del = 117,
		End = 119,
		Pagedown = 121,
		Up = 126,
		Down = 125,
		Left = 123,
		Right = 124,
	}

	static class KeyTranslator
	{
		internal static Key GetKey(CocoaKeyCode code)
		{
			// comments indicate members of the Key enum that are missing
			switch (code) {
				case CocoaKeyCode.A:
					return Key.A;
				case CocoaKeyCode.OptionAlt:
					return Key.AltLeft;
				// AltRight
				case CocoaKeyCode.B:
					return Key.B;
				case CocoaKeyCode.Backslash:
					return Key.BackSlash;
				case CocoaKeyCode.Backspace:
					return Key.BackSpace;
				case CocoaKeyCode.BracketLeft:
					return Key.BracketLeft;
				case CocoaKeyCode.BracketRight:
					return Key.BracketRight;
				case CocoaKeyCode.C:
					return Key.C;
				// Capslock
				// Clear
				case CocoaKeyCode.Comma:
					return Key.Comma;
				case CocoaKeyCode.Control:
					return Key.ControlLeft;
				// ControlRight
				case CocoaKeyCode.D:
					return Key.D;
				case CocoaKeyCode.Del:
					return Key.Delete;
				case CocoaKeyCode.Down:
					return Key.Down;
				case CocoaKeyCode.E:
					return Key.E;
				case CocoaKeyCode.End:
					return Key.End;
				case CocoaKeyCode.Enter:
					return Key.Enter;
				case CocoaKeyCode.Return:
					return Key.Enter;
				case CocoaKeyCode.Esc:
					return Key.Escape;
				case CocoaKeyCode.F:
					return Key.F;
				case CocoaKeyCode.F1:
					return Key.F1;
				case CocoaKeyCode.F2:
					return Key.F2;
				case CocoaKeyCode.F3:
					return Key.F3;
				case CocoaKeyCode.F4:
					return Key.F4;
				case CocoaKeyCode.F5:
					return Key.F5;
				case CocoaKeyCode.F6:
					return Key.F6;
				case CocoaKeyCode.F7:
					return Key.F7;
				case CocoaKeyCode.F8:
					return Key.F8;
				case CocoaKeyCode.F9:
					return Key.F9;
				case CocoaKeyCode.F10:
					return Key.F10;
				case CocoaKeyCode.F11:
					return Key.F11;
				case CocoaKeyCode.F12:
					return Key.F12;
				case CocoaKeyCode.F13:
					return Key.F13;
				case CocoaKeyCode.F14:
					return Key.F14;
				case CocoaKeyCode.F15:
					return Key.F15;
				// F16-F35
				case CocoaKeyCode.G:
					return Key.G;
				case CocoaKeyCode.H:
					return Key.H;
				case CocoaKeyCode.Home:
					return Key.Home;
				case CocoaKeyCode.I:
					return Key.I;
				case CocoaKeyCode.Insert:
					return Key.Insert;
				case CocoaKeyCode.J:
					return Key.J;
				case CocoaKeyCode.K:
					return Key.K;
				case CocoaKeyCode.KeyPad_0:
					return Key.Keypad0;
				case CocoaKeyCode.KeyPad_1:
					return Key.Keypad1;
				case CocoaKeyCode.KeyPad_2:
					return Key.Keypad2;
				case CocoaKeyCode.KeyPad_3:
					return Key.Keypad3;
				case CocoaKeyCode.KeyPad_4:
					return Key.Keypad4;
				case CocoaKeyCode.KeyPad_5:
					return Key.Keypad5;
				case CocoaKeyCode.KeyPad_6:
					return Key.Keypad6;
				case CocoaKeyCode.KeyPad_7:
					return Key.Keypad7;
				case CocoaKeyCode.KeyPad_8:
					return Key.Keypad8;
				case CocoaKeyCode.KeyPad_9:
					return Key.Keypad9;
				case CocoaKeyCode.KeyPad_Add:
					return Key.KeypadAdd;
				case CocoaKeyCode.KeyPad_Decimal:
					return Key.KeypadDecimal;
				case CocoaKeyCode.KeyPad_Divide:
					return Key.KeypadDivide;
				case CocoaKeyCode.KeyPad_Enter:
					return Key.KeypadEnter;
				case CocoaKeyCode.KeyPad_Multiply:
					return Key.KeypadMultiply;
				case CocoaKeyCode.KeyPad_Subtract:
					return Key.KeypadSubtract;
				//case MacOSKeyCode.KeyPad_Equal;
				case CocoaKeyCode.L:
					return Key.L;
				case CocoaKeyCode.Left:
					return Key.Left;
				case CocoaKeyCode.M:
					return Key.M;
				//Key.MaxKeys 
				case CocoaKeyCode.Menu:
					return Key.Menu;
				case CocoaKeyCode.Minus:
					return Key.Minus;
				case CocoaKeyCode.N:
					return Key.N;
				case CocoaKeyCode.Key_0:
					return Key.Number0;
				case CocoaKeyCode.Key_1:
					return Key.Number1;
				case CocoaKeyCode.Key_2:
					return Key.Number2;
				case CocoaKeyCode.Key_3:
					return Key.Number3;
				case CocoaKeyCode.Key_4:
					return Key.Number4;
				case CocoaKeyCode.Key_5:
					return Key.Number5;
				case CocoaKeyCode.Key_6:
					return Key.Number6;
				case CocoaKeyCode.Key_7:
					return Key.Number7;
				case CocoaKeyCode.Key_8:
					return Key.Number8;
				case CocoaKeyCode.Key_9:
					return Key.Number9;
				// Numlock
				case CocoaKeyCode.O:
					return Key.O;
				case CocoaKeyCode.P:
					return Key.P;
				case CocoaKeyCode.Pagedown:
					return Key.PageDown;
				case CocoaKeyCode.Pageup:
					return Key.PageUp;
				// Pause
				case CocoaKeyCode.Period:
					return Key.Period;
				case CocoaKeyCode.Equals:
					return Key.Plus;
				// PrintScreen
				case CocoaKeyCode.Q:
					return Key.Q;
				case CocoaKeyCode.Quote:
					return Key.Quote;
				case CocoaKeyCode.R:
					return Key.R;
				case CocoaKeyCode.Right:
					return Key.Right;
				case CocoaKeyCode.S:
					return Key.S;
				// ScrollLock
				case CocoaKeyCode.Semicolon:
					return Key.Semicolon;
				case CocoaKeyCode.Shift:
					return Key.ShiftLeft;
				//Key.ShiftRight 
				case CocoaKeyCode.Slash:
					return Key.Slash;
				// Key.Sleep
				case CocoaKeyCode.Space:
					return Key.Space;
				case CocoaKeyCode.T:
					return Key.T;
				case CocoaKeyCode.Tab:
					return Key.Tab;
				case CocoaKeyCode.Tilde:
					return Key.Tilde;
				case CocoaKeyCode.U:
					return Key.U;
				case CocoaKeyCode.Up:
					return Key.Up;
				case CocoaKeyCode.V:
					return Key.V;
				case CocoaKeyCode.W:
					return Key.W;
				case CocoaKeyCode.Command:
					return Key.WinLeft;
				// WinKeyRight
				case CocoaKeyCode.X:
					return Key.X;
				case CocoaKeyCode.Y:
					return Key.Y;
				case CocoaKeyCode.Z:
					return Key.Z;
				default:
					return Key.Unknown;
			}
		}
	}
}
#endif
