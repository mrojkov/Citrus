#if !UNITY
using System;
using System.Collections;
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

			public void OnBetweenFrames(float delta)
			{
				input.CopyKeysState();
				input.ProcessPendingKeyEvents(delta);
			}
		}

		public const int MaxTouches = 4;
		public float KeyRepeatDelay = 0.2f;
		public float KeyRepeatInterval = 0.03f;

		private struct KeyEvent
		{
			public Key Key;
			public bool State;
		}

		public InputSimulator Simulator;

		private Vector2[] touchPositions = new Vector2[MaxTouches];
		private List<KeyEvent> keyEventQueue = new List<KeyEvent>();

		private struct KeyState
		{
			public bool PreviousState;
			public bool CurrentState;
			public float RepeatDelay;
			public bool Repeated;
			public bool Disabled;
		}

		private KeyState[] keys = new KeyState[Key.MaxCount];

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
		public float WheelScrollAmount { get; private set; }

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
			return keys[key.Code].CurrentState;
		}

		public Modifiers GetModifiers()
		{
			var result = Modifiers.None;
			if (IsKeyPressed(Key.LShift) || IsKeyPressed(Key.RShift)) {
				result |= Modifiers.Shift;
			}
			if (IsKeyPressed(Key.LAlt) || IsKeyPressed(Key.RAlt)) {
				result |= Modifiers.Alt;
			}
			if (IsKeyPressed(Key.LControl) || IsKeyPressed(Key.RControl)) {
				result |= Modifiers.Control;
			}
			if (IsKeyPressed(Key.LWin) || IsKeyPressed(Key.RWin)) {
				result |= Modifiers.Win;
			}
			return result;
		}

		/// <summary>
		/// Returns true during the frame the user releases the key identified by name.
		/// </summary>
		public bool WasKeyReleased(Key key)
		{
			return !keys[key.Code].CurrentState && keys[key.Code].PreviousState;
		}

		/// <summary>
		/// Returns true during the frame the user starts pressing down the key identified by name.
		/// </summary>
		public bool WasKeyPressed(Key key)
		{
			return keys[key.Code].CurrentState && !keys[key.Code].PreviousState;
		}

		/// <summary>
		/// Returns true during the frame the user starts pressing down the key identified by name or key event was repeated.
		/// </summary>
		public bool WasKeyRepeated(Key key)
		{
			return keys[key.Code].Repeated;
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
			return WasKeyPressed(Key.Touch0 + index);
		}

		private Key GetMouseButtonByIndex(int button)
		{
			if (button < 0 || button > 2) {
				throw new ArgumentException();
			}
			return Key.Mouse0 + button;
		}

		public bool WasTouchEnded(int index)
		{
			return WasKeyReleased(Key.Touch0 + index);
		}

		public bool IsTouching(int index)
		{
			return IsKeyPressed(Key.Touch0 + index);
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

		public bool IsKeyEnabled(Key key)
		{
			return !keys[key].Disabled;
		}
		
		public void EnableKey(Key key, bool enable)
		{
			keys[key].Disabled = !enable;
		}

		internal void SetKeyState(Key key, bool value)
		{
			if (Key.Arrays.ModifierKeys[key]) {
				ReleaseAffectedByModifierKeys();
			}
			key = TranslateShortcuts(key);
			if (Key.Arrays.AffectedByModifiersKeys[key] && GetModifiers() != Modifiers.None) {
				return;
			}
			keyEventQueue.Add(new KeyEvent { Key = key, State = value });
		}

		private Key TranslateShortcuts(Key key)
		{
			var modifiers = GetModifiers();
			foreach (var kv in Key.ShortcutMap) {
				var shortcut = kv.Key;
				if (key == shortcut.Main && shortcut.Modifiers == modifiers) {
					key = kv.Value;
					break;
				}
			}
			return key;
		}

		private void ReleaseAffectedByModifierKeys()
		{
			for (var i = 0; i < Key.Arrays.AffectedByModifiersKeys.Count; i++) {
				if (keys[i].CurrentState && Key.Arrays.AffectedByModifiersKeys[i]) {
					SetKeyState(i, false);
				}
			}
		}

		internal bool HasPendingKeyEvent(Key key)
		{
			return keyEventQueue.Contains(new KeyEvent { Key = key, State = true }) ||
				keyEventQueue.Contains(new KeyEvent { Key = key, State = false });
		}

		internal void ProcessPendingKeyEvents(float delta)
		{
			for (int i = 0; i < Key.Count; i++) {
				var key = keys[i];
				key.Repeated = false;
				if (key.CurrentState) {
					if ((key.RepeatDelay -= delta) < 0) {
						key.RepeatDelay = KeyRepeatInterval;
						key.Repeated = true;
					}
				}
				keys[i] = key;
			}
			if (keyEventQueue.Count > 0) {
				var processedKeys = new BitArray(Key.MaxCount);
				for (int i = 0; i < keyEventQueue.Count; i++) {
					var evt = keyEventQueue[i];
					var k = evt.Key;
					if (!processedKeys[k]) {
						processedKeys[k] = true;
						keys[k].CurrentState = evt.State;
						keys[k].RepeatDelay = KeyRepeatDelay;
						keys[k].Repeated |= evt.State;
						keyEventQueue.RemoveAt(i);
						i--;
					}
				}
			}
		}

		internal void CopyKeysState()
		{
			for (int i = 0; i < Key.Count; i++) {
				keys[i].PreviousState = keys[i].CurrentState;
			}
		}

		internal void SetWheelScrollAmount(float delta)
		{
			if (delta == 0) {
				return;
			}
			var key = delta > 0 ? Key.MouseWheelUp : Key.MouseWheelDown;
			if (!HasPendingKeyEvent(key)) {
				SetKeyState(key, true);
				SetKeyState(key, false);
				WheelScrollAmount = delta;
			} else {
				WheelScrollAmount += delta;
			}
		}
	}
}

#endif