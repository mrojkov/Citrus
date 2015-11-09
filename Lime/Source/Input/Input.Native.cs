#if !UNITY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class Input
	{
		public class InputSimulator
		{
			Input input;

			public InputSimulator(Input input)
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

		public const int MaxTouches = 4;

		private struct KeyEvent
		{
			public Key Key;
			public bool State;
		}

		public InputSimulator Simulator;

		private Vector2[] touchPositions = new Vector2[MaxTouches];

		private List<KeyEvent> keyEventQueue = new List<KeyEvent>();

		public static readonly int KeyCount = Enum.GetNames(typeof(Key)).Length;

		private bool[] previousKeysState = new bool[KeyCount];
		private bool[] currentKeysState = new bool[KeyCount];

		/// <summary>
		/// The matrix describes transition from pixels to virtual coordinates.
		/// </summary>
		public Matrix32 ScreenToWorldTransform = Matrix32.Identity;

		/// <summary>
		/// The current mouse position in virtual coordinates coordinates. (read only)
		/// </summary>
		public Vector2 MousePosition { get; internal set; }

		/// <summary>
		/// Indicates how much the mouse wheel was moved
		/// </summary>
		public float WheelScrollAmount { get; internal set; }

		/// <summary>
		/// The current accelerometer state (read only) in g-force units
		/// </summary>
		public Vector3 Acceleration { get; internal set; }

		public Input()
		{
			Simulator = new InputSimulator(this);
		}

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

		public bool WasMousePressed()
		{
			return WasKeyPressed(GetMouseButtonByIndex(0));
		}

		public bool WasMouseReleased()
		{
			return WasKeyReleased(GetMouseButtonByIndex(0));
		}

		public bool IsMousePressed()
		{
			return IsKeyPressed(GetMouseButtonByIndex(0));
		}

		public bool WasMousePressed(int button)
		{
			return WasKeyPressed(GetMouseButtonByIndex(button));
		}

		public bool WasMouseReleased(int button)
		{
			return WasKeyReleased(GetMouseButtonByIndex(button));
		}

		public bool IsMousePressed(int button)
		{
			return IsKeyPressed(GetMouseButtonByIndex(button));
		}

		public bool WasTouchBegan(int index)
		{
			return WasKeyPressed((Key)((int)Key.Touch0 + index));
		}

		private Key GetMouseButtonByIndex(int button)
		{
			if (button < 0 || button > 2) {
				throw new ArgumentException();
			}
			return (Key)((int)Key.Mouse0 + button);
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
			keyEventQueue.Add(new KeyEvent { Key = key, State = value });
		}

		internal bool HasPendingKeyEvent(Key key)
		{
			return keyEventQueue.Contains(new KeyEvent { Key = key, State = true }) ||
				keyEventQueue.Contains(new KeyEvent { Key = key, State = false });
		}

		internal void ProcessPendingKeyEvents()
		{
			if (keyEventQueue.Count > 0) {
				var processedKeys = new bool[KeyCount];
				for (int i = 0; i < keyEventQueue.Count(); i++) {
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

		internal void CopyKeysState()
		{
			currentKeysState.CopyTo(previousKeysState, 0);
		}
	}
}

#endif
