#if !UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class Input
	{
		public const int MaxTouches = 4;

		struct KeyEvent
		{
			public Key Key;
			public bool State;
		}

		private static Vector2[] touchPositions = new Vector2[MaxTouches];

		private static List<KeyEvent> keyEventQueue = new List<KeyEvent>();
		
		static bool[] previousKeysState = new bool[(int)Key.KeyCount];
		static bool[] currentKeysState = new bool[(int)Key.KeyCount];

		/// <summary>
		/// The matrix describes transition from pixels to virtual coordinates.
		/// </summary>
		public static Matrix32 ScreenToWorldTransform = Matrix32.Identity;

		/// <summary>
		/// The current mouse position in virtual coordinates coordinates. (read only)
		/// </summary>
		public static Vector2 MousePosition { get; internal set; }

		/// <summary>
		/// The current accelerometer state (read only).
		/// </summary>
		public static Vector3 Acceleration { get; internal set; }

		/// <summary>
		/// Returns true while the user holds down the key identified by name. Think auto fire.
		/// </summary>
		public static bool IsKeyPressed(Key key)
		{
			return currentKeysState[(int)key];
		}

		/// <summary>
		/// Returns true during the frame the user releases the key identified by name.
		/// </summary>
		public static bool WasKeyReleased(Key key)
		{
			return !currentKeysState[(int)key] && previousKeysState[(int)key];
		}

		/// <summary>
		/// Returns true during the frame the user starts pressing down the key identified by name.
		/// </summary>
		public static bool WasKeyPressed(Key key)
		{
			return currentKeysState[(int)key] && !previousKeysState[(int)key];
		}

		public static bool WasMousePressed()
		{
			return WasKeyPressed(GetMouseButtonByIndex(0));
		}

		public static bool WasMouseReleased()
		{
			return WasKeyReleased(GetMouseButtonByIndex(0));
		}

		public static bool IsMousePressed()
		{
			return IsKeyPressed(GetMouseButtonByIndex(0));
		}

		public static bool WasMousePressed(int button)
		{
			return WasKeyPressed(GetMouseButtonByIndex(button));
		}

		public static bool WasMouseReleased(int button)
		{
			return WasKeyReleased(GetMouseButtonByIndex(button));
		}

		public static bool IsMousePressed(int button)
		{
			return IsKeyPressed(GetMouseButtonByIndex(button));
		}

		public static bool WasTouchBegan(int index)
		{
			return WasKeyPressed((Key)((int)Key.Touch0 + index));
		}

		private static Key GetMouseButtonByIndex(int button)
		{
			if (button < 0 || button > 2)
			{
				throw new ArgumentException();
			}
			return (Key)((int)Key.Mouse0 + button);
		}

		public static bool WasTouchEnded(int index)
		{
			return WasKeyReleased((Key)((int)Key.Touch0 + index));
		}

		public static bool IsTouching(int index)
		{
			return IsKeyPressed((Key)((int)Key.Touch0 + index));
		}

		public static Vector2 GetTouchPosition(int index)
		{
			return touchPositions[index];
		}

		internal static void SetTouchPosition(int index, Vector2 position)
		{
			touchPositions[index] = position;
		}

		public static int GetNumTouches()
		{
			int j = 0;
			for (int i = 0; i < MaxTouches; i++)
			{
				if (IsTouching(i))
					j++;
			}
			return j;
		}

		public static string TextInput { get; internal set; }

		internal static void SetKeyState(Key key, bool value)
		{
			keyEventQueue.Add(new KeyEvent { Key = key, State = value });
		}

		internal static void ProcessPendingKeyEvents()
		{
			if (keyEventQueue.Count > 0)
			{
				var processedKeys = new bool[(int)Key.KeyCount];
				for (int i = 0; i < keyEventQueue.Count(); i++)
				{
					var evt = keyEventQueue[i];
					if (!processedKeys[(int)evt.Key])
					{
						processedKeys[(int)evt.Key] = true;
						currentKeysState[(int)evt.Key] = evt.State;
						keyEventQueue.RemoveAt(i);
						i--;
					}
				}
			}
		}

		internal static void CopyKeysState()
		{
			currentKeysState.CopyTo(previousKeysState, 0);
		}
	}
}
#endif