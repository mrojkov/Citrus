using System;
using System.Collections.Generic;

namespace Lime
{
	public class WindowInput
	{
		private readonly IWindow ownerWindow;

		private Vector2 lastDesktopMousePosition = new Vector2(-1, -1);
		private Vector2 calculatedMousePosition = new Vector2(-1, -1);

		public WindowInput(IWindow ownerWindow)
		{
			this.ownerWindow = ownerWindow;
		}

		public bool Changed => Application.Input.Changed;

		internal bool IsSimulationRunning { get; set; }

		private Matrix32 mousePositionTransform = Matrix32.Identity;

		public Matrix32 MousePositionTransform
		{
			get { return mousePositionTransform; }
			set
			{
				mousePositionTransform = value;
				Array.Clear(calculatedTouchPositions, 0, Input.MaxTouches);
				lastDesktopMousePosition = new Vector2(-1, -1);
			}
		}

		/// <summary>
		/// The current mouse position in local coordinates. (read only)
		/// </summary>
		public Vector2 MousePosition
		{
			get
			{
				if (lastDesktopMousePosition != Application.Input.DesktopMousePosition) {
					lastDesktopMousePosition = Application.Input.DesktopMousePosition;
					calculatedMousePosition = MouseDesktopToLocal(Application.Input.DesktopMousePosition);
				}
				return calculatedMousePosition;
			}
		}

		/// <summary>
		/// Converts transformed local mouse coordinates into desktop mouse coordinates.
		/// </summary>
		public Vector2 MouseLocalToDesktop(Vector2 localMousePosition) => ownerWindow.LocalToDesktop(localMousePosition * MousePositionTransform.CalcInversed());

		/// <summary>
		/// Converts desktop mouse coordinates into transformed local mouse coordinates.
		/// </summary>
		public Vector2 MouseDesktopToLocal(Vector2 desktopMousePosition) => ownerWindow.DesktopToLocal(desktopMousePosition) * MousePositionTransform;

		/// <summary>
		/// Indicates how much the mouse wheel was moved
		/// </summary>
		public float WheelScrollAmount => (ownerWindow == Application.WindowUnderMouse) ? Application.Input.WheelScrollAmount : 0;

		/// <summary>
		/// Returns true while the user holds down the key identified by name. Think auto fire.
		/// </summary>
		public bool IsKeyPressed(Key key) => ValidateKey(key) && Application.Input.IsKeyPressed(key);

		public Modifiers GetModifiers() => !ownerWindow.Active ? Modifiers.None : Application.Input.GetModifiers();

		/// <summary>
		/// Returns true during the frame the user releases the key identified by name.
		/// </summary>
		public bool WasKeyReleased(Key key) => ValidateKey(key) && Application.Input.WasKeyReleased(key);

		/// <summary>
		/// Returns true during the frame the user starts pressing down the key identified by name.
		/// </summary>
		public bool WasKeyPressed(Key key) => ValidateKey(key) && Application.Input.WasKeyPressed(key);

		public void ConsumeKey(Key key) => Application.Input.ConsumeKey(key);

		/// <summary>
		/// Returns true during the frame the user starts pressing down the key identified by name or key event was repeated.
		/// </summary>
		public bool WasKeyRepeated(Key key) => ValidateKey(key) && Application.Input.WasKeyRepeated(key);
		public bool WasMousePressed() => WasKeyPressed(Input.GetMouseButtonByIndex(0));
		public bool WasMouseReleased() => WasKeyReleased(Input.GetMouseButtonByIndex(0));
		public bool IsMousePressed() => IsKeyPressed(Input.GetMouseButtonByIndex(0));
		public bool WasMousePressed(int button) => WasKeyPressed(Input.GetMouseButtonByIndex(button));
		public bool WasMouseReleased(int button) => WasKeyReleased(Input.GetMouseButtonByIndex(button));
		public bool IsMousePressed(int button) => IsKeyPressed(Input.GetMouseButtonByIndex(button));
		public bool WasTouchBegan(int index) => WasKeyPressed(Input.GetTouchByIndex(index));
		public bool WasTouchEnded(int index) => WasKeyReleased(Input.GetTouchByIndex(index));
		public bool IsTouching(int index) => IsKeyPressed(Input.GetTouchByIndex(index));
		public bool WasDoubleClickPressed(int buttonIndex) => WasKeyPressed(Input.GetDoubleClickByIndex(buttonIndex));

		private Vector2?[] calculatedTouchPositions = new Vector2?[Input.MaxTouches];
		private readonly Vector2[] lastTouchPositions = new Vector2[Input.MaxTouches];

		public Vector2 GetTouchPosition(int index)
		{
			var desktopTouchPosition = Application.Input.GetDesktopTouchPosition(index);
			if (calculatedTouchPositions[index] == null || lastTouchPositions[index] != desktopTouchPosition) {
				lastTouchPositions[index] = desktopTouchPosition;
				calculatedTouchPositions[index] = MouseDesktopToLocal(desktopTouchPosition);
			}
			return calculatedTouchPositions[index].Value;
		}

		public int GetNumTouches() => !ownerWindow.Active ? 0 : Application.Input.GetNumTouches();

		public string TextInput
		{
			get { return Application.Input.TextInput; }
			internal set { Application.Input.TextInput = value; }
		}

		internal void SetKeyState(Key key, bool value) => Application.Input.SetKeyState(key, value);
		internal void ProcessPendingKeyEvents(float delta) => Application.Input.ProcessPendingKeyEvents(delta);
		internal void ProcessPendingInputEvents(float delta) => Application.Input.ProcessPendingInputEvents(delta);
		internal void CopyKeysState() => Application.Input.CopyKeysState();
		internal void ClearKeyState(bool clearMouseButtons = true) => Application.Input.ClearKeyState(clearMouseButtons);
		internal void SetWheelScrollAmount(float delta) => Application.Input.SetWheelScrollAmount(delta);

		private bool ValidateKey(Key key)
		{
			if (IsSimulationRunning) {
				return true;
			}
			return (key.IsMouseKey())
				? ownerWindow == Application.WindowUnderMouse
				: ownerWindow.Active;
		}
		/// <summary>
		/// Tries to get drop data.
		/// </summary>
		/// <param name="dropData">Drop data.</param>
		/// <returns></returns>
		public bool TryGetDropData(out IEnumerable<string> dropData) => Application.Input.TryGetDropData(out dropData);
		/// <summary>
		/// Consumes a single dropped data object.
		/// </summary>
		/// <param name="dataObject">Dropped data object to be consumed.</param>
		public void ConsumeDropData(string dataObject) => Application.Input.ConsumeDropData(dataObject);
		/// <summary>
		/// Consumes dropped data.
		/// </summary>
		/// <param name="dropData">Dropped data objects to be consumed.</param>
		public void ConsumeDropData(IEnumerable<string> dropData) => Application.Input.ConsumeDropData(dropData);
	}
}
