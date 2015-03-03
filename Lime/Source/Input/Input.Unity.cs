#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class Input
	{
		struct KeyEvent
		{
			public Key Key;
			public bool State;
		}

		private static Vector2[] touchPositions = new Vector2[MaxTouches];

		private static List<KeyEvent> keyEventQueue = new List<KeyEvent>();
		
		public const int MaxTouches = 4;

		static bool[] previousKeysState = new bool[(int)Key.KeyCount];
		static bool[] currentKeysState = new bool[(int)Key.KeyCount];

		/// <summary>
		/// The matrix describes transition from pixels to virtual coordinates.
		/// </summary>
		public static Matrix32 ScreenToWorldTransform = Matrix32.Identity;

		/// <summary>
		/// The current mouse position in virtual coordinates coordinates. (read only)
		/// </summary>
		public static Vector2 MousePosition { get; private set; }

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

		public static bool WasMousePressed(int button = 0)
		{
			return WasKeyPressed((Key)((int)Key.Mouse0 + button));
		}

		public static bool WasMouseReleased(int button = 0)
		{
			return WasKeyReleased((Key)((int)Key.Mouse0 + button));
		}

		public static bool IsMousePressed(int button = 0)
		{
			return IsKeyPressed((Key)((int)Key.Mouse0 + button));
		}

		public static bool WasTouchBegan(int index)
		{
			return WasKeyPressed((Key)((int)Key.Touch0 + index));
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
			for (int i = 0; i < MaxTouches; i++) {
				if (IsTouching(i))
					j++;
			}
			return j;
		}
		
		public static string TextInput { get; internal set; }

		internal static void SetKeyState(Key key, bool value)
		{
			keyEventQueue.Add(new KeyEvent{Key = key, State = value});
		}
		
		internal static void ProcessPendingKeyEvents()
		{
			if (keyEventQueue.Count > 0) {
				var processedKeys = new bool[(int)Key.KeyCount];
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

		public static void Refresh()
		{
			Input.TextInput = null;
			Input.CopyKeysState();
			RefreshMousePosition();
			currentKeysState[(int)Key.Mouse0] = UnityEngine.Input.GetMouseButton(0);
			currentKeysState[(int)Key.Mouse1] = UnityEngine.Input.GetMouseButton(1);
			currentKeysState[(int)Key.Mouse2] = UnityEngine.Input.GetMouseButton(2);
		}

		private static void RefreshMousePosition()
		{
			var p = UnityEngine.Input.mousePosition;
			MousePosition = new Vector2(p.x, UnityEngine.Screen.height - p.y);
			MousePosition *= ScreenToWorldTransform;
		}

		internal static void CopyKeysState()
		{
			currentKeysState.CopyTo(previousKeysState, 0);
		}
	}
}
#endif