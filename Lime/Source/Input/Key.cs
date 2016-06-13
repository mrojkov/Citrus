namespace Lime
{
	public struct Key
	{
		public readonly int Value;

		public static int Count { get; private set; }
		public const int MaxCount = 256;

		static Key() { Count = 142; }
		public Key(int id) { Value = id; }

		public static Key New() { return new Key(Count++); }

		public static explicit operator Key (int id) { return new Key(id); }
		public static explicit operator int (Key key) { return key.Value; }

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Value == ((Key)obj).Value;
		}

		public static bool operator ==(Key lhs, Key rhs)
		{
			return lhs.Value == rhs.Value;
		}

		public static bool operator !=(Key lhs, Key rhs)
		{
			return lhs.Value != rhs.Value;
		}

		public static readonly Key Unknown = new Key(0);
		public static readonly Key LShift = new Key(1);
		public static readonly Key ShiftLeft = new Key(1);
		public static readonly Key RShift = new Key(2);
		public static readonly Key ShiftRight = new Key(2);
		public static readonly Key LControl = new Key(3);
		public static readonly Key ControlLeft = new Key(3);
		public static readonly Key RControl = new Key(4);
		public static readonly Key ControlRight = new Key(4);
		public static readonly Key AltLeft = new Key(5);
		public static readonly Key LAlt = new Key(5);
		public static readonly Key AltRight = new Key(6);
		public static readonly Key RAlt = new Key(6);
		public static readonly Key WinLeft = new Key(7);
		public static readonly Key LWin = new Key(7);
		public static readonly Key RWin = new Key(8);
		public static readonly Key WinRight = new Key(8);
		public static readonly Key Menu = new Key(9);
		public static readonly Key F1 = new Key(10);
		public static readonly Key F2 = new Key(11);
		public static readonly Key F3 = new Key(12);
		public static readonly Key F4 = new Key(13);
		public static readonly Key F5 = new Key(14);
		public static readonly Key F6 = new Key(15);
		public static readonly Key F7 = new Key(16);
		public static readonly Key F8 = new Key(17);
		public static readonly Key F9 = new Key(18);
		public static readonly Key F10 = new Key(19);
		public static readonly Key F11 = new Key(20);
		public static readonly Key F12 = new Key(21);
		public static readonly Key F13 = new Key(22);
		public static readonly Key F14 = new Key(23);
		public static readonly Key F15 = new Key(24);
		public static readonly Key F16 = new Key(25);
		public static readonly Key F17 = new Key(26);
		public static readonly Key F18 = new Key(27);
		public static readonly Key F19 = new Key(28);
		public static readonly Key F20 = new Key(29);
		public static readonly Key F21 = new Key(30);
		public static readonly Key F22 = new Key(31);
		public static readonly Key F23 = new Key(32);
		public static readonly Key F24 = new Key(33);
		public static readonly Key F25 = new Key(34);
		public static readonly Key F26 = new Key(35);
		public static readonly Key F27 = new Key(36);
		public static readonly Key F28 = new Key(37);
		public static readonly Key F29 = new Key(38);
		public static readonly Key F30 = new Key(39);
		public static readonly Key F31 = new Key(40);
		public static readonly Key F32 = new Key(41);
		public static readonly Key F33 = new Key(42);
		public static readonly Key F34 = new Key(43);
		public static readonly Key F35 = new Key(44);
		public static readonly Key Up = new Key(45);
		public static readonly Key Down = new Key(46);
		public static readonly Key Left = new Key(47);
		public static readonly Key Right = new Key(48);
		public static readonly Key Enter = new Key(49);
		public static readonly Key Escape = new Key(50);
		public static readonly Key Space = new Key(51);
		public static readonly Key Tab = new Key(52);
		public static readonly Key Back = new Key(53);
		public static readonly Key BackSpace = new Key(53);
		public static readonly Key Insert = new Key(54);
		public static readonly Key Delete = new Key(55);
		public static readonly Key PageUp = new Key(56);
		public static readonly Key PageDown = new Key(57);
		public static readonly Key Home = new Key(58);
		public static readonly Key End = new Key(59);
		public static readonly Key CapsLock = new Key(60);
		public static readonly Key ScrollLock = new Key(61);
		public static readonly Key PrintScreen = new Key(62);
		public static readonly Key Pause = new Key(63);
		public static readonly Key NumLock = new Key(64);
		public static readonly Key Clear = new Key(65);
		public static readonly Key Sleep = new Key(66);
		public static readonly Key Keypad0 = new Key(67);
		public static readonly Key Keypad1 = new Key(68);
		public static readonly Key Keypad2 = new Key(69);
		public static readonly Key Keypad3 = new Key(70);
		public static readonly Key Keypad4 = new Key(71);
		public static readonly Key Keypad5 = new Key(72);
		public static readonly Key Keypad6 = new Key(73);
		public static readonly Key Keypad7 = new Key(74);
		public static readonly Key Keypad8 = new Key(75);
		public static readonly Key Keypad9 = new Key(76);
		public static readonly Key KeypadDivide = new Key(77);
		public static readonly Key KeypadMultiply = new Key(78);
		public static readonly Key KeypadMinus = new Key(79);
		public static readonly Key KeypadSubtract = new Key(79);
		public static readonly Key KeypadAdd = new Key(80);
		public static readonly Key KeypadPlus = new Key(80);
		public static readonly Key KeypadDecimal = new Key(81);
		public static readonly Key KeypadEnter = new Key(82);
		public static readonly Key A = new Key(83);
		public static readonly Key B = new Key(84);
		public static readonly Key C = new Key(85);
		public static readonly Key D = new Key(86);
		public static readonly Key E = new Key(87);
		public static readonly Key F = new Key(88);
		public static readonly Key G = new Key(89);
		public static readonly Key H = new Key(90);
		public static readonly Key I = new Key(91);
		public static readonly Key J = new Key(92);
		public static readonly Key K = new Key(93);
		public static readonly Key L = new Key(94);
		public static readonly Key M = new Key(95);
		public static readonly Key N = new Key(96);
		public static readonly Key O = new Key(97);
		public static readonly Key P = new Key(98);
		public static readonly Key Q = new Key(99);
		public static readonly Key R = new Key(100);
		public static readonly Key S = new Key(101);
		public static readonly Key T = new Key(102);
		public static readonly Key U = new Key(103);
		public static readonly Key V = new Key(104);
		public static readonly Key W = new Key(105);
		public static readonly Key X = new Key(106);
		public static readonly Key Y = new Key(107);
		public static readonly Key Z = new Key(108);
		public static readonly Key Number0 = new Key(109);
		public static readonly Key Number1 = new Key(110);
		public static readonly Key Number2 = new Key(111);
		public static readonly Key Number3 = new Key(112);
		public static readonly Key Number4 = new Key(113);
		public static readonly Key Number5 = new Key(114);
		public static readonly Key Number6 = new Key(115);
		public static readonly Key Number7 = new Key(116);
		public static readonly Key Number8 = new Key(117);
		public static readonly Key Number9 = new Key(118);
		public static readonly Key Tilde = new Key(119);
		public static readonly Key Minus = new Key(120);
		public static readonly Key Plus = new Key(121);
		public static readonly Key LBracket = new Key(122);
		public static readonly Key BracketLeft = new Key(122);
		public static readonly Key BracketRight = new Key(123);
		public static readonly Key RBracket = new Key(123);
		public static readonly Key Semicolon = new Key(124);
		public static readonly Key Quote = new Key(125);
		public static readonly Key Comma = new Key(126);
		public static readonly Key Period = new Key(127);
		public static readonly Key Slash = new Key(128);
		public static readonly Key BackSlash = new Key(129);

		/// <summary>
		/// Left mouse button
		/// </summary>
		public static readonly Key Mouse0 = new Key(130);
		/// <summary>
		/// Right mouse button
		/// </summary>
		public static readonly Key Mouse1 = new Key(131);
		/// <summary>
		/// Middle mouse button
		/// </summary>
		public static readonly Key Mouse2 = new Key(132);

		public static readonly Key Touch0 = new Key(133);
		public static readonly Key Touch1 = new Key(134);
		public static readonly Key Touch2 = new Key(135);
		public static readonly Key Touch3 = new Key(136);

		public static readonly Key MouseWheelUp = new Key(137);
		public static readonly Key MouseWheelDown = new Key(138);
		public static readonly Key DismissSoftKeyboard = new Key(139);

		/// <summary>
		/// The double click with the left mouse button.
		/// </summary>
		public static readonly Key Mouse0DoubleClick = new Key(140);

		/// <summary>
		/// The double click with the right mouse button.
		/// </summary>
		public static readonly Key Mouse1DoubleClick = new Key(141);
	}
}

