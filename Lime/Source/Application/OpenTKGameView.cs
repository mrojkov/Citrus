#if WIN
using System;
using OpenTK;
using OpenTK.Input;

namespace Lime
{
	public class GameView : OpenTK.GameWindow
	{
		public static GameView Instance;
		Application app;
		public bool PowerSaveMode { get; set; }
		internal static event Action DidUpdated;

		public GameView(Application app, string[] args = null)
			: base(640, 480, new OpenTK.Graphics.GraphicsMode(32, 0, 0, 1))
		{
			Instance = this;
			this.app = app;
			app.Active = true;
			AudioSystem.Initialize();
			app.OnCreate();
			this.Keyboard.KeyDown += HandleKeyDown;
			this.Keyboard.KeyUp += HandleKeyUp;
			this.KeyPress += HandleKeyPress;
			this.Mouse.ButtonDown += HandleMouseButtonDown;
			this.Mouse.ButtonUp += HandleMouseButtonUp;
			this.Mouse.Move += HandleMouseMove;
			this.Mouse.WheelChanged += HandleMouseWheel;
			SetupWindowLocationAndSize(args);
			PowerSaveMode = CheckPowerSaveFlag(args);
		}

		private void SetupWindowLocationAndSize(string[] args)
		{
			var displayBounds = OpenTK.DisplayDevice.Default.Bounds;
			if (CheckFullscreenArg(args)) {
				this.WindowState = OpenTK.WindowState.Fullscreen;
			} else if (CheckMaximizedFlag(args)) {
				this.Location = displayBounds.Location;
				this.WindowState = OpenTK.WindowState.Maximized;
			} else {
				this.Location = new System.Drawing.Point {
					X = (displayBounds.Width - this.Width) / 2 + displayBounds.X,
					Y = (displayBounds.Height - this.Height) / 2 + displayBounds.Y
				};
			}
		}

		private static bool CheckMaximizedFlag(string[] args)
		{
			return args != null && Array.IndexOf(args, "--Maximized") >= 0;
		}

		private static bool CheckPowerSaveFlag(string[] args)
		{
			return args != null && Array.IndexOf(args, "--PowerSave") >= 0;
		}

		private static bool CheckFullscreenArg(string[] args)
		{
			return args != null && Array.IndexOf(args, "--Fullscreen") >= 0;
		}

		void HandleKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			Input.SetKeyState((Key)e.Key, true);
		}

		void HandleKeyUp(object sender, KeyboardKeyEventArgs e)
		{
			Input.SetKeyState((Key)e.Key, false);
		}

		void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			Input.TextInput += e.KeyChar;
		}

		protected override void OnFocusedChanged(EventArgs e)
		{
			Application.Instance.Active = this.Focused;
			if (this.Focused) {
				Application.Instance.OnActivate();
			} else {
				Application.Instance.OnDeactivate();
			}
		}

		void HandleMouseButtonUp(object sender, MouseButtonEventArgs e)
		{
			switch(e.Button) {
			case OpenTK.Input.MouseButton.Left:
				Input.SetKeyState(Key.Mouse0, false);
				Input.SetKeyState(Key.Touch0, false);
				break;
			case OpenTK.Input.MouseButton.Right:
				Input.SetKeyState(Key.Mouse1, false);
				break;
			case OpenTK.Input.MouseButton.Middle:
				Input.SetKeyState(Key.Mouse2, false);
				break;
			}
		}

		void HandleMouseButtonDown(object sender, MouseButtonEventArgs e)
		{
			switch(e.Button) {
			case OpenTK.Input.MouseButton.Left:
				Input.SetKeyState(Key.Mouse0, true);
				Input.SetKeyState(Key.Touch0, true);
				break;
			case OpenTK.Input.MouseButton.Right:
				Input.SetKeyState(Key.Mouse1, true);
				break;
			case OpenTK.Input.MouseButton.Middle:
				Input.SetKeyState(Key.Mouse2, true);
				break;
			}
		}

		void HandleMouseMove(object sender, MouseMoveEventArgs e)
		{
			Vector2 position = new Vector2(e.X, e.Y) * Input.ScreenToWorldTransform;
			Input.MousePosition = position;
			Input.SetTouchPosition(0, position);
		}

		void HandleMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0) {
				Input.SetKeyState(Key.MouseWheelUp, true);
				Input.SetKeyState(Key.MouseWheelUp, false);
			} else if (e.Delta < 0) {
				Input.SetKeyState(Key.MouseWheelDown, true);
				Input.SetKeyState(Key.MouseWheelDown, false);
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			TexturePool.Instance.DiscardAllTextures();
			AudioSystem.Terminate();
		}

		private long lastMillisecondsCount = 0;

		protected override void OnRenderFrame(OpenTK.FrameEventArgs e)
		{
			long millisecondsCount = ApplicationToolbox.GetMillisecondsSinceGameStarted();
			int delta = (int)(millisecondsCount - lastMillisecondsCount);
			delta = delta.Clamp(0, 40);
			lastMillisecondsCount = millisecondsCount;
			Update(delta);
			if (DidUpdated != null) {
				DidUpdated();
			}
			Render();
			if (PowerSaveMode) {
				millisecondsCount = ApplicationToolbox.GetMillisecondsSinceGameStarted();
				delta = (int)(millisecondsCount - lastMillisecondsCount);
				System.Threading.Thread.Sleep(Math.Max(0, (1000 / 25) - delta));
			}
		}

		private void Render()
		{
			ApplicationToolbox.RefreshFrameRate();
			MakeCurrent();
			app.OnRenderFrame();
			SwapBuffers();
		}

		private void Update(float delta)
		{
			Input.ProcessPendingKeyEvents();
			Input.MouseVisible = true;
			app.OnUpdateFrame((int)delta);
			Input.TextInput = null;
			Input.CopyKeysState();
		}
		
		public Size WindowSize { 
			get { return new Size(ClientSize.Width, ClientSize.Height); } 
			set { this.ClientSize = new System.Drawing.Size(value.Width, value.Height); } 
		}
		
		public bool FullScreen { 
			get { 
				return this.WindowState == WindowState.Fullscreen;
			}
			set { 
				this.WindowState = value ? WindowState.Fullscreen : WindowState.Normal;
			}
		}

		public float FrameRate { 
			get { return ApplicationToolbox.FrameRate; } 
		}
	}
}
#endif