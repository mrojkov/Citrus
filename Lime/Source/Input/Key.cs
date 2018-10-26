using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public struct Key
	{
		public const int MaxCount = 512;

		public static int Count { get; private set; } = 1;
		public static Key New() { return Count++; }

		public static readonly Key Unknown = 0;
		public static readonly Dictionary<Shortcut, Key> ShortcutMap = new Dictionary<Shortcut, Key>();

		public readonly int Code;

		public Key(int code) { Code = code; }

		public bool IsMouseKey() => this >= Mouse0 && this <= Touch3;

		public bool IsDoubleClickKey() => this == Mouse0DoubleClick || this == Mouse1DoubleClick;

		public bool IsModifier() => this >= Shift && this <= Win;

		public bool IsAlphanumeric() => IsLetter() || IsDigit();

		public bool IsDigit() => this >= Number0 && this <= Number9;

		public bool IsLetter() => this >= A && this <= Z;

		public bool IsTextNavigation() =>
			this == PageUp || this == PageDown || this == Home || this == End ||
			this == Left || this == Right || this == Up || this == Down;

		public bool IsPrintable() =>
			IsAlphanumeric() || (this >= Tilde && this <= BackSlash) || this == Space;

		public bool IsTextEditing() =>
			this == Delete || this == BackSpace || this == Insert || this == Enter;

		public bool IsFunctional() =>this >= F1 && this <= F12;

		public static bool operator == (Key lhs, Key rhs) => lhs.Code == rhs.Code;
		public static bool operator != (Key lhs, Key rhs) => lhs.Code != rhs.Code;
		public static bool operator >  (Key lhs, Key rhs) => lhs.Code >  rhs.Code;
		public static bool operator <  (Key lhs, Key rhs) => lhs.Code <  rhs.Code;
		public static bool operator >= (Key lhs, Key rhs) => lhs.Code >= rhs.Code;
		public static bool operator <= (Key lhs, Key rhs) => lhs.Code <= rhs.Code;

		public static IEnumerable<Key> Enumerate()
		{
			for (int i = 0; i < Count; i++) {
				yield return (Key)i;
			}
		}

		public override int GetHashCode() => Code;
		public override bool Equals(object obj) => (Key)obj == this;

		public static Key GetByName(string name) =>
			keyToNameCache.FirstOrDefault(p => p.Value == name).Key;

		public override string ToString()
		{
			string value;
			if (keyToNameCache.TryGetValue(this, out value)) {
				return value;
			}

			var sb = new StringBuilder(10);
			foreach (var kv in ShortcutMap) {
				if (kv.Value == this)
					sb.Append($"[{kv.Key.ToString()}]");
			}
			return sb.Length > 0 ? sb.ToString() : Code.ToString();
		}

		private static Dictionary<Key, string> keyToNameCache;

		static Key()
		{
			keyToNameCache = typeof(Key).GetFields()
				.Where(i => i.FieldType == typeof(Key) && i.Name != nameof(Key.LastNormal))
				.ToDictionary(i => (Key)i.GetValue(null), i => i.Name);
		}

		public static implicit operator Key (int code) => new Key(code);
		public static implicit operator int (Key key) => key.Code;

		public static Key MapShortcut(Key main) =>
			MapShortcut(new Shortcut(Modifiers.None, main));

		public static Key MapShortcut(Modifiers modifiers, Key main) =>
			MapShortcut(new Shortcut(modifiers, main));

		public static Key MapShortcut(Shortcut shortcut)
		{
			if (!Shortcut.ValidateMainKey(shortcut.Main)) {
				throw new ArgumentException();
			}
			Key key;
			if (!ShortcutMap.TryGetValue(shortcut, out key)) {
				key = New();
				ShortcutMap.Add(shortcut, key);
			}
			return key;
		}

		public void AddAlias(Modifiers modifiers, Key main) =>
			AddAlias(new Shortcut(modifiers, main));

		public void AddAlias(Shortcut shortcut)
		{
			if (!Shortcut.ValidateMainKey(shortcut.Main)) {
				throw new ArgumentException();
			}
			Key key;
			if (!ShortcutMap.TryGetValue(shortcut, out key))
				ShortcutMap.Add(shortcut, this);
			else if (key != this)
				throw new InvalidOperationException();
		}

#region Keyboard
		public static readonly Key Shift = New();
		public static readonly Key Control = New();
		public static readonly Key Alt = New();
		public static readonly Key Win = New();
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
		public static readonly Key EqualsSign = New();
		public static readonly Key LBracket = New();
		public static readonly Key RBracket = New();
		public static readonly Key Semicolon = New();
		public static readonly Key Quote = New();
		public static readonly Key Comma = New();
		public static readonly Key Period = New();
		public static readonly Key Slash = New();
		public static readonly Key BackSlash = New();

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
		public static readonly Key KeypadMultiply = New();
		public static readonly Key KeypadPlus = New();
		public static readonly Key KeypadMinus = New();
		public static readonly Key KeypadDecimal = New();
		public static readonly Key KeypadDivide = New();

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

		public static readonly Key MouseWheelUp = New();
		public static readonly Key MouseWheelDown = New();

		public static readonly Key MouseBack = New();
		public static readonly Key MouseForward = New();

		/// <summary>
		/// The double click with the left mouse button.
		/// </summary>
		public static readonly Key Mouse0DoubleClick = New();

		/// <summary>
		/// The double click with the right mouse button.
		/// </summary>
		public static readonly Key Mouse1DoubleClick = New();

		public static readonly Key Touch0 = New();
		public static readonly Key Touch1 = New();
		public static readonly Key Touch2 = New();
		public static readonly Key Touch3 = New();
#endregion

		public static readonly Key DismissSoftKeyboard = New();

		public static readonly Key LastNormal = Count - 1;
	}
}

