using Lime;

namespace ChromiumWebBrowser
{
	internal static class CefButtonKeyMap
	{
		public static CefKey? GetButton(Key key)
		{
			switch (key)
			{
				#region Letters
				case Key.A:
					return CefKey.A;
				case Key.B:
					return CefKey.B;
				case Key.C:
					return CefKey.C;
				case Key.D:
					return CefKey.D;
				case Key.E:
					return CefKey.E;
				case Key.F:
					return CefKey.F;
				case Key.G:
					return CefKey.G;
				case Key.H:
					return CefKey.H;
				case Key.I:
					return CefKey.I;
				case Key.J:
					return CefKey.J;
				case Key.K:
					return CefKey.K;
				case Key.L:
					return CefKey.L;
				case Key.M:
					return CefKey.M;
				case Key.N:
					return CefKey.N;
				case Key.O:
					return CefKey.O;
				case Key.P:
					return CefKey.P;
				case Key.Q:
					return CefKey.Q;
				case Key.R:
					return CefKey.R;
				case Key.S:
					return CefKey.S;
				case Key.T:
					return CefKey.T;
				case Key.U:
					return CefKey.U;
				case Key.V:
					return CefKey.V;
				case Key.W:
					return CefKey.W;
				case Key.X:
					return CefKey.X;
				case Key.Y:
					return CefKey.Y;
				case Key.Z:
					return CefKey.Z;
				#endregion

				#region Numbers
				case Key.Number1:
					return CefKey.Number1;
				case Key.Number2:
					return CefKey.Number2;
				case Key.Number3:
					return CefKey.Number3;
				case Key.Number4:
					return CefKey.Number4;
				case Key.Number5:
					return CefKey.Number5;
				case Key.Number6:
					return CefKey.Number6;
				case Key.Number7:
					return CefKey.Number7;
				case Key.Number8:
					return CefKey.Number8;
				case Key.Number9:
					return CefKey.Number9;
				case Key.Number0:
					return CefKey.Number0;
				#endregion

				#region Symbols
					case Key.Space:
						return CefKey.Space;
					case Key.Enter:
						return CefKey.Return;
					case Key.Semicolon:
						return CefKey.Semicolon;
					case Key.Comma:
						return CefKey.Comma;
					case Key.Period:
						return CefKey.Period;
					case Key.Plus:
						return CefKey.Plus;
					case Key.Minus:
						return CefKey.Minus;
					case Key.Slash:
						return CefKey.Slash;
					case Key.Tilde:
						return CefKey.Tilde;
					case Key.LBracket:
						return CefKey.LBracket;
					case Key.BackSlash:
						return CefKey.BackSlash;
					case Key.RBracket:
						return CefKey.RBracket;
					case Key.Quote:
						return CefKey.Quote;
				#endregion

				#region Keypad
				case Key.Keypad0:
					return CefKey.Numpad0;
				case Key.Keypad1:
					return CefKey.Numpad1;
				case Key.Keypad2:
					return CefKey.Numpad2;
				case Key.Keypad3:
					return CefKey.Numpad3;
				case Key.Keypad4:
					return CefKey.Numpad4;
				case Key.Keypad5:
					return CefKey.Numpad5;
				case Key.Keypad6:
					return CefKey.Numpad6;
				case Key.Keypad7:
					return CefKey.Numpad7;
				case Key.Keypad8:
					return CefKey.Numpad8;
				case Key.Keypad9:
					return CefKey.Numpad9;
				case Key.KeypadAdd:
					return CefKey.Add;
				case Key.KeypadDecimal:
					return CefKey.Decimal;
				case Key.KeypadDivide:
					return CefKey.Divide;
				case Key.KeypadEnter:
					return CefKey.Return;
				case Key.KeypadSubtract:
					return CefKey.Subtract;
				case Key.KeypadMultiply:
					return CefKey.Multiply;
				#endregion
					
				#region System Keys
					case Key.Escape:
						return CefKey.Escape;
					case Key.BackSpace:
						return CefKey.Back;
					case Key.Tab:
						return CefKey.Tab;
					case Key.PrintScreen:
						return CefKey.Snapshot;
					case Key.Pause:
						return CefKey.Pause;
					case Key.PageDown:
						return CefKey.Next;
					case Key.PageUp:
						return CefKey.Prior;
					case Key.End:
						return CefKey.End;
					case Key.Home:
						return CefKey.Home;
					case Key.Insert:
						return CefKey.Insert;
					case Key.Delete:
						return CefKey.Delete;
					case Key.CapsLock:
						return CefKey.Capital;
					case Key.ScrollLock:
						return CefKey.Scroll;
					case Key.NumLock:
						return CefKey.Numlock;
					case Key.RShift:
						return CefKey.RShift;
					case Key.LShift:
						return CefKey.LShift;
					case Key.RControl:
						return CefKey.RControl;
					case Key.LControl:
						return CefKey.LControl;
					case Key.RAlt:
						return CefKey.RAlt;
					case Key.LAlt:
						return CefKey.LAlt;
					case Key.RWin:
						return CefKey.Rwin;
					case Key.LWin:
						return CefKey.Lwin;
				#endregion

				#region Arrows
					case Key.Left:
						return CefKey.Left;
					case Key.Right:
						return CefKey.Right;
					case Key.Up:
						return CefKey.Up;
					case Key.Down:
						return CefKey.Down;
				#endregion

				#region F buttons
				case Key.F1:
					return CefKey.F1;
				case Key.F2:
					return CefKey.F2;
				case Key.F3:
					return CefKey.F3;
				case Key.F4:
					return CefKey.F4;
				case Key.F5:
					return CefKey.F5;
				case Key.F6:
					return CefKey.F6;
				case Key.F7:
					return CefKey.F7;
				case Key.F8:
					return CefKey.F8;
				case Key.F9:
					return CefKey.F9;
				case Key.F10:
					return CefKey.F10;
				case Key.F11:
					return CefKey.F11;
				case Key.F12:
					return CefKey.F12;
				#endregion

				default:
					return null;
			}
		}
	}
}
