#if WIN
using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;

namespace Lime
{
	public class GameView : OpenTK.GameWindow
	{
		private Application app;

		public static GameView Instance;
		// Indicates whether the game uses OpenGL or OpenGL ES 2.0
		public RenderingApi RenderingApi { get; private set; }
		public bool PowerSaveMode { get; set; }
		internal static event Action DidUpdated;

		public GameView(Application app, string[] args = null)
			: base(800, 600, GraphicsMode.Default, 
			"Citrus", GameWindowFlags.Default, DisplayDevice.Default,
			2, 0, GetGraphicContextFlags(args))
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
			RenderingApi = GetRenderingApi(args);
		}

		private static GraphicsContextFlags GetGraphicContextFlags(string[] args)
		{
			return GetRenderingApi(args) == RenderingApi.OpenGL ? 
				 GraphicsContextFlags.Default : GraphicsContextFlags.Embedded;
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
					X = Math.Max(0, (displayBounds.Width - this.Width) / 2 + displayBounds.X),
					Y = Math.Max(0, (displayBounds.Height - this.Height) / 2 + displayBounds.Y)
				};
			}
		}

		private static bool CheckMaximizedFlag(string[] args)
		{
			return args != null && Array.IndexOf(args, "--Maximized") >= 0;
		}

		private static RenderingApi GetRenderingApi(string[] args)
		{
			return RenderingApi.ES20;
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
			// SDL backend bug: OpenTK doesn't send key press event for backspace
			if (e.Key == OpenTK.Input.Key.BackSpace) {
				Input.TextInput += '\b';
			}
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
			app.OnTerminate();
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
			app.OnUpdateFrame((int)delta);
            AudioSystem.Update();
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