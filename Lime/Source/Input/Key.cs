using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public struct Key
	{
		public static class Arrays
		{
			public static readonly BitArray AllKeys = FromRange(0, MaxCount - 1);
			public static readonly BitArray KeyboardKeys = FromRange(LShift, BackSlash);
			public static readonly BitArray ModifierKeys = FromRange(LShift, Menu);
			public static readonly BitArray AffectedByModifiersKeys = FromRange(F1, BackSlash);
			public static readonly BitArray MouseButtons = FromRange(Mouse0, Mouse1DoubleClick);

			static BitArray FromRange(int min, int max)
			{
				var r = new BitArray(Key.MaxCount);
				for (int i = min; i <= max; i++) {
					r.Set(i, true);
				}
				return r;
			}
		}

		public static Key GetByName(string name)
		{
			var field = typeof(Key).GetFields().
				FirstOrDefault(i => String.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase) && i.FieldType == typeof(Key));
			return field != null ? (Key)field.GetValue(null) : Key.Unknown;
		}

		public const int MaxCount = 512;

		public static int Count = 150;
		public static Key New() { return Count++; }

		public static readonly Key Unknown = 0;
		public static readonly Dictionary<Shortcut, Key> ShortcutMap = new Dictionary<Shortcut, Key>();

		public readonly int Code;

		public Key(int code) { Code = code; }

		public static implicit operator Key (int code) { return new Key(code); }
		public static implicit operator int (Key key) { return key.Code; }

		public static Key MapShortcut(Modifiers modifiers, Key main)
		{
			return MapShortcut(new Shortcut(modifiers, main));
		}

		public static Key MapShortcut(Shortcut shortcut)
		{
			if (!Arrays.AffectedByModifiersKeys[shortcut.Main]) {
				throw new ArgumentException();
			}
			if (shortcut.Modifiers == Modifiers.None) {
				return shortcut.Main;
			}
			Key key;
			if (!ShortcutMap.TryGetValue(shortcut, out key)) {
				key = New();
				ShortcutMap.Add(shortcut, key);
			}
			return key;
		}

#region Keyboard
		public static readonly Key LShift = 1;
		public static readonly Key ShiftLeft = 1;
		public static readonly Key RShift = 2;
		public static readonly Key ShiftRight = 2;
		public static readonly Key LControl = 3;
		public static readonly Key ControlLeft = 3;
		public static readonly Key RControl = 4;
		public static readonly Key ControlRight = 4;
		public static readonly Key AltLeft = 5;
		public static readonly Key LAlt = 5;
		public static readonly Key AltRight = 6;
		public static readonly Key RAlt = 6;
		public static readonly Key WinLeft = 7;
		public static readonly Key LWin = 7;
		public static readonly Key RWin = 8;
		public static readonly Key WinRight = 8;
		public static readonly Key Menu = 9;
		public static readonly Key F1 = 10;
		public static readonly Key F2 = 11;
		public static readonly Key F3 = 12;
		public static readonly Key F4 = 13;
		public static readonly Key F5 = 14;
		public static readonly Key F6 = 15;
		public static readonly Key F7 = 16;
		public static readonly Key F8 = 17;
		public static readonly Key F9 = 18;
		public static readonly Key F10 = 19;
		public static readonly Key F11 = 20;
		public static readonly Key F12 = 21;
		public static readonly Key F13 = 22;
		public static readonly Key F14 = 23;
		public static readonly Key F15 = 24;
		public static readonly Key F16 = 25;
		public static readonly Key F17 = 26;
		public static readonly Key F18 = 27;
		public static readonly Key F19 = 28;
		public static readonly Key F20 = 29;
		public static readonly Key F21 = 30;
		public static readonly Key F22 = 31;
		public static readonly Key F23 = 32;
		public static readonly Key F24 = 33;
		public static readonly Key F25 = 34;
		public static readonly Key F26 = 35;
		public static readonly Key F27 = 36;
		public static readonly Key F28 = 37;
		public static readonly Key F29 = 38;
		public static readonly Key F30 = 39;
		public static readonly Key F31 = 40;
		public static readonly Key F32 = 41;
		public static readonly Key F33 = 42;
		public static readonly Key F34 = 43;
		public static readonly Key F35 = 44;
		public static readonly Key Up = 45;
		public static readonly Key Down = 46;
		public static readonly Key Left = 47;
		public static readonly Key Right = 48;
		public static readonly Key Enter = 49;
		public static readonly Key Escape = 50;
		public static readonly Key Space = 51;
		public static readonly Key Tab = 52;
		public static readonly Key Back = 53;
		public static readonly Key BackSpace = 53;
		public static readonly Key Insert = 54;
		public static readonly Key Delete = 55;
		public static readonly Key PageUp = 56;
		public static readonly Key PageDown = 57;
		public static readonly Key Home = 58;
		public static readonly Key End = 59;
		public static readonly Key CapsLock = 60;
		public static readonly Key ScrollLock = 61;
		public static readonly Key PrintScreen = 62;
		public static readonly Key Pause = 63;
		public static readonly Key NumLock = 64;
		public static readonly Key Clear = 65;
		public static readonly Key Sleep = 66;
		public static readonly Key Keypad0 = 67;
		public static readonly Key Keypad1 = 68;
		public static readonly Key Keypad2 = 69;
		public static readonly Key Keypad3 = 70;
		public static readonly Key Keypad4 = 71;
		public static readonly Key Keypad5 = 72;
		public static readonly Key Keypad6 = 73;
		public static readonly Key Keypad7 = 74;
		public static readonly Key Keypad8 = 75;
		public static readonly Key Keypad9 = 76;
		public static readonly Key KeypadDivide = 77;
		public static readonly Key KeypadMultiply = 78;
		public static readonly Key KeypadMinus = 79;
		public static readonly Key KeypadSubtract = 79;
		public static readonly Key KeypadAdd = 80;
		public static readonly Key KeypadPlus = 80;
		public static readonly Key KeypadDecimal = 81;
		public static readonly Key KeypadEnter = 82;
		public static readonly Key A = 83;
		public static readonly Key B = 84;
		public static readonly Key C = 85;
		public static readonly Key D = 86;
		public static readonly Key E = 87;
		public static readonly Key F = 88;
		public static readonly Key G = 89;
		public static readonly Key H = 90;
		public static readonly Key I = 91;
		public static readonly Key J = 92;
		public static readonly Key K = 93;
		public static readonly Key L = 94;
		public static readonly Key M = 95;
		public static readonly Key N = 96;
		public static readonly Key O = 97;
		public static readonly Key P = 98;
		public static readonly Key Q = 99;
		public static readonly Key R = 100;
		public static readonly Key S = 101;
		public static readonly Key T = 102;
		public static readonly Key U = 103;
		public static readonly Key V = 104;
		public static readonly Key W = 105;
		public static readonly Key X = 106;
		public static readonly Key Y = 107;
		public static readonly Key Z = 108;
		public static readonly Key Number0 = 109;
		public static readonly Key Number1 = 110;
		public static readonly Key Number2 = 111;
		public static readonly Key Number3 = 112;
		public static readonly Key Number4 = 113;
		public static readonly Key Number5 = 114;
		public static readonly Key Number6 = 115;
		public static readonly Key Number7 = 116;
		public static readonly Key Number8 = 117;
		public static readonly Key Number9 = 118;
		public static readonly Key Tilde = 119;
		public static readonly Key Minus = 120;
		public static readonly Key Plus = 121;
		public static readonly Key LBracket = 122;
		public static readonly Key BracketLeft = 122;
		public static readonly Key BracketRight = 123;
		public static readonly Key RBracket = 123;
		public static readonly Key Semicolon = 124;
		public static readonly Key Quote = 125;
		public static readonly Key Comma = 126;
		public static readonly Key Period = 127;
		public static readonly Key Slash = 128;
		public static readonly Key BackSlash = 129;
#endregion

#region Mouse
		/// <summary>
		/// Left mouse button
		/// </summary>
		public static readonly Key Mouse0 = 130;
		/// <summary>
		/// Right mouse button
		/// </summary>
		public static readonly Key Mouse1 = 131;
		/// <summary>
		/// Middle mouse button
		/// </summary>
		public static readonly Key Mouse2 = 132;

		public static readonly Key Touch0 = 133;
		public static readonly Key Touch1 = 134;
		public static readonly Key Touch2 = 135;
		public static readonly Key Touch3 = 136;

		public static readonly Key MouseWheelUp = 137;
		public static readonly Key MouseWheelDown = 138;
		public static readonly Key DismissSoftKeyboard = 139;

		/// <summary>
		/// The double click with the left mouse button.
		/// </summary>
		public static readonly Key Mouse0DoubleClick = 140;

		/// <summary>
		/// The double click with the right mouse button.
		/// </summary>
		public static readonly Key Mouse1DoubleClick = 141;
#endregion

#region Miscellaneous
		public static readonly Key Undo = New();
		public static readonly Key Redo = New();
#endregion
	}
}

