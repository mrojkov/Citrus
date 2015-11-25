#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class Input
	{
		public class Simulator
		{
			Input input;
			public Simulator(Input input)
			{
				this.input = input;
			}

			public void SetMousePosition(Vector2 position)
			{
				input.MousePosition = position;
			}

			public void SetKeyState(Key key, bool value)
			{
				input.SetKeyState(key, value);
			}

			public void OnBetweenFrames()
			{
				input.CopyKeysState();
				input.ProcessPendingKeyEvents();
			}
		}

		struct KeyEvent
		{
			public Key Key;
			public bool State;
		}

		private Vector2[] touchPositions = new Vector2[MaxTouches];

		private List<KeyEvent> keyEventQueue = new List<KeyEvent>();
		
		public const int MaxTouches = 4;

		bool[] previousKeysState = new bool[Enum.GetNames(typeof(Key)).Length];
		bool[] currentKeysState = new bool[Enum.GetNames(typeof(Key)).Length];

		/// <summary>
		/// The matrix describes transition from pixels to virtual coordinates.
		/// </summary>
		public Matrix32 ScreenToWorldTransform = Matrix32.Identity;

		/// <summary>
		/// The current mouse position in virtual coordinates coordinates. (read only)
		/// </summary>
		public Vector2 MousePosition { get; private set; }

		/// <summary>
		/// Indicates how much the mouse wheel was moved
		/// </summary>
		public float WheelScrollAmount { get; internal set; }

		/// <summary>
		/// The current accelerometer state (read only).
		/// </summary>
		public Vector3 Acceleration { get; internal set; }

		/// <summary>
		/// Returns true while the user holds down the key identified by name. Think auto fire.
		/// </summary>
		public bool IsKeyPressed(Key key)
		{
			return currentKeysState[(int)key];
		}

		/// <summary>
		/// Returns true during the frame the user releases the key identified by name.
		/// </summary>
		public bool WasKeyReleased(Key key)
		{
			return !currentKeysState[(int)key] && previousKeysState[(int)key];
		}

		/// <summary>
		/// Returns true during the frame the user starts pressing down the key identified by name.
		/// </summary>
		public bool WasKeyPressed(Key key)
		{
			return currentKeysState[(int)key] && !previousKeysState[(int)key];
		}

		public bool WasMousePressed(int button = 0)
		{
			return WasKeyPressed((Key)((int)Key.Mouse0 + button));
		}

		public bool WasMouseReleased(int button = 0)
		{
			return WasKeyReleased((Key)((int)Key.Mouse0 + button));
		}

		public bool IsMousePressed(int button = 0)
		{
			return IsKeyPressed((Key)((int)Key.Mouse0 + button));
		}

		public bool WasTouchBegan(int index)
		{
			return WasKeyPressed((Key)((int)Key.Touch0 + index));
		}

		public bool WasTouchEnded(int index)
		{
			return WasKeyReleased((Key)((int)Key.Touch0 + index));
		}

		public bool IsTouching(int index)
		{
			return IsKeyPressed((Key)((int)Key.Touch0 + index));
		}

		public Vector2 GetTouchPosition(int index)
		{
			return touchPositions[index];
		}

		internal void SetTouchPosition(int index, Vector2 position)
		{
			touchPositions[index] = position;
		}
		
		public int GetNumTouches()
		{
			int j = 0;
			for (int i = 0; i < MaxTouches; i++) {
				if (IsTouching(i))
					j++;
			}
			return j;
		}
		
		public string TextInput { get; internal set; }

		internal void SetKeyState(Key key, bool value)
		{
			keyEventQueue.Add(new KeyEvent{Key = key, State = value});
		}
		
		internal void ProcessPendingKeyEvents()
		{
			if (keyEventQueue.Count > 0) {
				var processedKeys = new bool[Enum.GetNames(typeof(Key)).Length];
				for (int i = 0; i < keyEventQueue.Count; i++) {
					var evt = keyEventQueue[i];
					if (!processedKeys[(int)evt.Key]) {
						processedKeys[(int)evt.Key] = true;
						currentKeysState[(int)evt.Key] = evt.State;
						keyEventQueue.RemoveAt(i);
						i--;
					}
				}
			}
		}

		public void Refresh()
		{
			TextInput = null;
			CopyKeysState();
			RefreshMousePosition();
			currentKeysState[(int)Key.Mouse0] = UnityEngine.Input.GetMouseButton(0);
			currentKeysState[(int)Key.Mouse1] = UnityEngine.Input.GetMouseButton(1);
			currentKeysState[(int)Key.Mouse2] = UnityEngine.Input.GetMouseButton(2);
			currentKeysState[(int)Key.Touch0] = UnityEngine.Input.GetMouseButton(0);
			GrabKeyState(UnityEngine.KeyCode.LeftShift, Key.ShiftLeft);
			GrabKeyState(UnityEngine.KeyCode.RightShift, Key.ShiftRight);
			GrabKeyState(UnityEngine.KeyCode.LeftControl, Key.ControlLeft);
			GrabKeyState(UnityEngine.KeyCode.RightControl, Key.ControlRight);
			GrabKeyState(UnityEngine.KeyCode.LeftAlt, Key.AltLeft);
			GrabKeyState(UnityEngine.KeyCode.RightAlt, Key.AltRight);
			GrabKeyState(UnityEngine.KeyCode.LeftWindows, Key.WinLeft);
			GrabKeyState(UnityEngine.KeyCode.RightWindows, Key.WinRight);
			GrabKeyState(UnityEngine.KeyCode.Menu, Key.Menu);
			GrabKeyState(UnityEngine.KeyCode.F1, Key.F1);
			GrabKeyState(UnityEngine.KeyCode.F2, Key.F1);
			GrabKeyState(UnityEngine.KeyCode.F3, Key.F2);
			GrabKeyState(UnityEngine.KeyCode.F4, Key.F3);
			GrabKeyState(UnityEngine.KeyCode.F5, Key.F5);
			GrabKeyState(UnityEngine.KeyCode.F6, Key.F6);
			GrabKeyState(UnityEngine.KeyCode.F7, Key.F7);
			GrabKeyState(UnityEngine.KeyCode.F8, Key.F8);
			GrabKeyState(UnityEngine.KeyCode.F9, Key.F9);
			GrabKeyState(UnityEngine.KeyCode.F10, Key.F10);
			GrabKeyState(UnityEngine.KeyCode.F11, Key.F11);
			GrabKeyState(UnityEngine.KeyCode.F12, Key.F12);
			GrabKeyState(UnityEngine.KeyCode.LeftArrow, Key.Left);
			GrabKeyState(UnityEngine.KeyCode.RightArrow, Key.Right);
			GrabKeyState(UnityEngine.KeyCode.UpArrow, Key.Up);
			GrabKeyState(UnityEngine.KeyCode.DownArrow, Key.Down);
			GrabKeyState(UnityEngine.KeyCode.Return, Key.Enter);
			GrabKeyState(UnityEngine.KeyCode.Escape, Key.Escape);
			GrabKeyState(UnityEngine.KeyCode.Space, Key.Space);
			GrabKeyState(UnityEngine.KeyCode.Tab, Key.Tab);
			GrabKeyState(UnityEngine.KeyCode.Backspace, Key.BackSpace);
			GrabKeyState(UnityEngine.KeyCode.Insert, Key.Insert);
			GrabKeyState(UnityEngine.KeyCode.Delete, Key.Delete);
			GrabKeyState(UnityEngine.KeyCode.PageUp, Key.PageUp);
			GrabKeyState(UnityEngine.KeyCode.PageDown, Key.PageDown);
			GrabKeyState(UnityEngine.KeyCode.Home, Key.Home);
			GrabKeyState(UnityEngine.KeyCode.End, Key.End);
			GrabKeyState(UnityEngine.KeyCode.CapsLock, Key.CapsLock);
			GrabKeyState(UnityEngine.KeyCode.Keypad0, Key.Keypad0);
			GrabKeyState(UnityEngine.KeyCode.Keypad1, Key.Keypad1);
			GrabKeyState(UnityEngine.KeyCode.Keypad2, Key.Keypad2);
			GrabKeyState(UnityEngine.KeyCode.Keypad3, Key.Keypad3);
			GrabKeyState(UnityEngine.KeyCode.Keypad4, Key.Keypad4);
			GrabKeyState(UnityEngine.KeyCode.Keypad5, Key.Keypad5);
			GrabKeyState(UnityEngine.KeyCode.Keypad6, Key.Keypad6);
			GrabKeyState(UnityEngine.KeyCode.Keypad7, Key.Keypad7);
			GrabKeyState(UnityEngine.KeyCode.Keypad8, Key.Keypad8);
			GrabKeyState(UnityEngine.KeyCode.Keypad9, Key.Keypad9);
			GrabKeyState(UnityEngine.KeyCode.KeypadDivide, Key.KeypadDivide);
			GrabKeyState(UnityEngine.KeyCode.KeypadMultiply, Key.KeypadMultiply);
			GrabKeyState(UnityEngine.KeyCode.KeypadMinus, Key.KeypadMinus);
			GrabKeyState(UnityEngine.KeyCode.KeypadPlus, Key.KeypadPlus);
			GrabKeyState(UnityEngine.KeyCode.KeypadEnter, Key.KeypadEnter);
			GrabKeyState(UnityEngine.KeyCode.Q, Key.Q);
			GrabKeyState(UnityEngine.KeyCode.W, Key.W);
			GrabKeyState(UnityEngine.KeyCode.E, Key.E);
			GrabKeyState(UnityEngine.KeyCode.R, Key.R);
			GrabKeyState(UnityEngine.KeyCode.T, Key.T);
			GrabKeyState(UnityEngine.KeyCode.Y, Key.Y);
			GrabKeyState(UnityEngine.KeyCode.U, Key.U);
			GrabKeyState(UnityEngine.KeyCode.I, Key.I);
			GrabKeyState(UnityEngine.KeyCode.O, Key.O);
			GrabKeyState(UnityEngine.KeyCode.P, Key.P);
			GrabKeyState(UnityEngine.KeyCode.A, Key.A);
			GrabKeyState(UnityEngine.KeyCode.S, Key.S);
			GrabKeyState(UnityEngine.KeyCode.D, Key.D);
			GrabKeyState(UnityEngine.KeyCode.F, Key.F);
			GrabKeyState(UnityEngine.KeyCode.G, Key.G);
			GrabKeyState(UnityEngine.KeyCode.H, Key.H);
			GrabKeyState(UnityEngine.KeyCode.J, Key.J);
			GrabKeyState(UnityEngine.KeyCode.K, Key.K);
			GrabKeyState(UnityEngine.KeyCode.L, Key.L);
			GrabKeyState(UnityEngine.KeyCode.Z, Key.Z);
			GrabKeyState(UnityEngine.KeyCode.X, Key.X);
			GrabKeyState(UnityEngine.KeyCode.C, Key.C);
			GrabKeyState(UnityEngine.KeyCode.V, Key.V);
			GrabKeyState(UnityEngine.KeyCode.B, Key.B);
			GrabKeyState(UnityEngine.KeyCode.N, Key.N);
			GrabKeyState(UnityEngine.KeyCode.M, Key.M);
			GrabKeyState(UnityEngine.KeyCode.Alpha0, Key.Number0);
			GrabKeyState(UnityEngine.KeyCode.Alpha1, Key.Number1);
			GrabKeyState(UnityEngine.KeyCode.Alpha2, Key.Number2);
			GrabKeyState(UnityEngine.KeyCode.Alpha3, Key.Number3);
			GrabKeyState(UnityEngine.KeyCode.Alpha4, Key.Number4);
			GrabKeyState(UnityEngine.KeyCode.Alpha5, Key.Number5);
			GrabKeyState(UnityEngine.KeyCode.Alpha6, Key.Number6);
			GrabKeyState(UnityEngine.KeyCode.Alpha7, Key.Number7);
			GrabKeyState(UnityEngine.KeyCode.Alpha8, Key.Number8);
			GrabKeyState(UnityEngine.KeyCode.Alpha9, Key.Number9);
			GrabKeyState(UnityEngine.KeyCode.AltGr, Key.Tilde);
			GrabKeyState(UnityEngine.KeyCode.Minus, Key.Minus);
			GrabKeyState(UnityEngine.KeyCode.Plus, Key.Plus);
			GrabKeyState(UnityEngine.KeyCode.LeftBracket, Key.LBracket);
			GrabKeyState(UnityEngine.KeyCode.RightBracket, Key.RBracket);
			GrabKeyState(UnityEngine.KeyCode.Semicolon, Key.Semicolon);
			GrabKeyState(UnityEngine.KeyCode.Quote, Key.Quote);
			GrabKeyState(UnityEngine.KeyCode.Comma, Key.Comma);
			GrabKeyState(UnityEngine.KeyCode.Period, Key.Period);
			GrabKeyState(UnityEngine.KeyCode.Slash, Key.Slash);
			GrabKeyState(UnityEngine.KeyCode.Backslash, Key.BackSlash);
		}

		private void GrabKeyState(UnityEngine.KeyCode unityKey, Key ourKey)
		{
			currentKeysState[(int)ourKey] = UnityEngine.Input.GetKey(unityKey);
		}

		private void RefreshMousePosition()
		{
			var p = UnityEngine.Input.mousePosition;
			MousePosition = new Vector2(p.x, UnityEngine.Screen.height - p.y);
			MousePosition *= ScreenToWorldTransform;
		}

		internal void CopyKeysState()
		{
			currentKeysState.CopyTo(previousKeysState, 0);
		}
	}
}
#endif