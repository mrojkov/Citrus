using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public struct Key
	{
		public static Key GetByName(string name)
		{
			var field = typeof(Key).GetFields().
				FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase) && i.FieldType == typeof(Key));
			return (Key?) field?.GetValue(null) ?? Key.Unknown;
		}

		public static string Name(Key key)
		{
			var field = typeof(Key).GetFields().FirstOrDefault(i => i.FieldType == typeof(Key) && Equals(((Key)i.GetValue(null)).Code, key.Code));
			return field?.Name ?? "Unknown";
		}

		public const int MaxCount = 512;

		public static int Count = 1;
		public static Key New() { return Count++; }

		public static readonly Key Unknown = 0;
		public static readonly Dictionary<Shortcut, Key> ShortcutMap = new Dictionary<Shortcut, Key>();

		public readonly int Code;

		public Key(int code) { Code = code; }

		public bool IsMouseButton()
		{
			return Code >= Mouse0.Code && Code <= Mouse1DoubleClick.Code;
		}

		public bool IsAffectedByModifiers()
		{
			return Code >= F1.Code && Code <= BackSlash.Code;
		}

		public bool IsModifier()
		{
			return Code >= LShift.Code && Code <= Menu.Code;
		}

		public static IEnumerable<Key> Enumerate()
		{
			for (int i = 0; i < Count; i++) {
				yield return (Key)i;
			}
		}

		public static implicit operator Key (int code) { return new Key(code); }
		public static implicit operator int (Key key) { return key.Code; }

		public static Key MapShortcut(Modifiers modifiers, Key main)
		{
			return MapShortcut(new Shortcut(modifiers, main));
		}

		public static Key MapShortcut(Shortcut shortcut)
		{
			if (!shortcut.Main.IsAffectedByModifiers()) {
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
		public static readonly Key LShift = New();
		public static readonly Key RShift = New();
		public static readonly Key LControl = New();
		public static readonly Key RControl = New();
#if MAC
		public static readonly Key Command = New();
#else
		public static readonly Key Command = LControl;
#endif
		public static readonly Key LAlt = New();
		public static readonly Key RAlt = New();
		public static readonly Key LWin = New();
		public static readonly Key RWin = New();
		public static readonly Key Menu = New();
		public static readonly Key F1 = New();
		public static readonly Key F2 = New();
		public static readonly Key F3 = New();
		public static readonly Key F4 = New();
		public static readonly Key F5 = New();
		public static readonly Key F6 = New();
		public static readonly Key F7 = New();
		public static readonly Key F8 = New();
		public static readonly Key F9 = New();
		public static readonly Key F10 = New();
		public static readonly Key F11 = New();
		public static readonly Key F12 = New();
		public static readonly Key F13 = New();
		public static readonly Key F14 = New();
		public static readonly Key F15 = New();
		public static readonly Key F16 = New();
		public static readonly Key F17 = New();
		public static readonly Key F18 = New();
		public static readonly Key F19 = New();
		public static readonly Key F20 = New();
		public static readonly Key F21 = New();
		public static readonly Key F22 = New();
		public static readonly Key F23 = New();
		public static readonly Key F24 = New();
		public static readonly Key F25 = New();
		public static readonly Key F26 = New();
		public static readonly Key F27 = New();
		public static readonly Key F28 = New();
		public static readonly Key F29 = New();
		public static readonly Key F30 = New();
		public static readonly Key F31 = New();
		public static readonly Key F32 = New();
		public static readonly Key F33 = New();
		public static readonly Key F34 = New();
		public static readonly Key F35 = New();
		public static readonly Key Up = New();
		public static readonly Key Down = New();
		public static readonly Key Left = New();
		public static readonly Key Right = New();
		public static readonly Key Enter = New();
		public static readonly Key Escape = New();
		public static readonly Key Space = New();
		public static readonly Key Tab = New();
		public static readonly Key Back = New();
		public static readonly Key BackSpace = New();
		public static readonly Key Insert = New();
		public static readonly Key Delete = New();
		public static readonly Key PageUp = New();
		public static readonly Key PageDown = New();
		public static readonly Key Home = New();
		public static readonly Key End = New();
		public static readonly Key CapsLock = New();
		public static readonly Key ScrollLock = New();
		public static readonly Key PrintScreen = New();
		public static readonly Key Pause = New();
		public static readonly Key NumLock = New();
		public static readonly Key Clear = New();
		public static readonly Key Sleep = New();
		public static readonly Key Keypad0 = New();
		public static readonly Key Keypad1 = New();
		public static readonly Key Keypad2 = New();
		public static readonly Key Keypad3 = New();
		public static readonly Key Keypad4 = New();
		public static readonly Key Keypad5 = New();
		public static readonly Key Keypad6 = New();
		public static readonly Key Keypad7 = New();
		public static readonly Key Keypad8 = New();
		public static readonly Key Keypad9 = New();
		public static readonly Key KeypadDivide = New();
		public static readonly Key KeypadMultiply = New();
		public static readonly Key KeypadMinus = New();
		public static readonly Key KeypadSubtract = New();
		public static readonly Key KeypadAdd = New();
		public static readonly Key KeypadPlus = New();
		public static readonly Key KeypadDecimal = New();
		public static readonly Key KeypadEnter = New();
		public static readonly Key A = New();
		public static readonly Key B = New();
		public static readonly Key C = New();
		public static readonly Key D = New();
		public static readonly Key E = New();
		public static readonly Key F = New();
		public static readonly Key G = New();
		public static readonly Key H = New();
		public static readonly Key I = New();
		public static readonly Key J = New();
		public static readonly Key K = New();
		public static readonly Key L = New();
		public static readonly Key M = New();
		public static readonly Key N = New();
		public static readonly Key O = New();
		public static readonly Key P = New();
		public static readonly Key Q = New();
		public static readonly Key R = New();
		public static readonly Key S = New();
		public static readonly Key T = New();
		public static readonly Key U = New();
		public static readonly Key V = New();
		public static readonly Key W = New();
		public static readonly Key X = New();
		public static readonly Key Y = New();
		public static readonly Key Z = New();
		public static readonly Key Number0 = New();
		public static readonly Key Number1 = New();
		public static readonly Key Number2 = New();
		public static readonly Key Number3 = New();
		public static readonly Key Number4 = New();
		public static readonly Key Number5 = New();
		public static readonly Key Number6 = New();
		public static readonly Key Number7 = New();
		public static readonly Key Number8 = New();
		public static readonly Key Number9 = New();
		public static readonly Key Tilde = New();
		public static readonly Key Minus = New();
		public static readonly Key Plus = New();
		public static readonly Key LBracket = New();
		public static readonly Key BracketLeft = New();
		public static readonly Key BracketRight = New();
		public static readonly Key RBracket = New();
		public static readonly Key Semicolon = New();
		public static readonly Key Quote = New();
		public static readonly Key Comma = New();
		public static readonly Key Period = New();
		public static readonly Key Slash = New();
		public static readonly Key BackSlash = New();
#endregion

#region Mouse
		/// <summary>
		/// Left mouse button
		/// </summary>
		public static readonly Key Mouse0 = New();
		/// <summary>
		/// Right mouse button
		/// </summary>
		public static readonly Key Mouse1 = New();
		/// <summary>
		/// Middle mouse button
		/// </summary>
		public static readonly Key Mouse2 = New();

		public static readonly Key Touch0 = New();
		public static readonly Key Touch1 = New();
		public static readonly Key Touch2 = New();
		public static readonly Key Touch3 = New();

		public static readonly Key MouseWheelUp = New();
		public static readonly Key MouseWheelDown = New();
		public static readonly Key DismissSoftKeyboard = New();

		/// <summary>
		/// The double click with the left mouse button.
		/// </summary>
		public static readonly Key Mouse0DoubleClick = New();

		/// <summary>
		/// The double click with the right mouse button.
		/// </summary>
		public static readonly Key Mouse1DoubleClick = New();
#endregion

		public static class Commands
		{
			public static readonly Key Undo = New();
			public static readonly Key Redo = New();
			public static readonly Key SelectAll = New();
			public static readonly Key Cut = New();
			public static readonly Key Copy = New();
			public static readonly Key Paste = New();
			public static readonly Key Delete = New();
		}
	}
}

