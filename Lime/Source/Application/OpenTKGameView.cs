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

		public GameView(Application app, string[] args = null)
			: base(640, 480, new OpenTK.Graphics.GraphicsMode(32, 0, 0, 1))
		{
			Instance = this;
			this.app = app;
			app.Active = true;
			AudioSystem.Initialize(16);
			app.OnCreate();
			this.Keyboard.KeyDown += HandleKeyDown;
			this.Keyboard.KeyUp += HandleKeyUp;
			this.KeyPress += HandleKeyPress;
			this.Mouse.ButtonDown += HandleMouseButtonDown;
			this.Mouse.ButtonUp += HandleMouseButtonUp;
			this.Mouse.Move += HandleMouseMove;

			// Как узнать разрешение текущего экрана без Windows Forms?
            Size screenSize = new Size(1280, 1024);

			if (CheckFullscreenArg(args)) {
				this.WindowState = OpenTK.WindowState.Fullscreen;
			} else if (CheckMaximizedArg(args)) {
				this.Location = new System.Drawing.Point(0, 0);
				this.WindowState = OpenTK.WindowState.Maximized;
			} else {
				this.Location = new System.Drawing.Point(
					(screenSize.Width - this.Width) / 2,
					(screenSize.Height - this.Height) / 2
				);
			}
		}

		private static bool CheckMaximizedArg(string[] args)
		{
			return args != null && Array.IndexOf(args, "--Maximized") >= 0;
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

		protected override void OnClosed(EventArgs e)
		{
			TexturePool.Instance.DiscardAllTextures();
			AudioSystem.Terminate();
		}

		private long prevTime = 0;

		protected override void OnRenderFrame(OpenTK.FrameEventArgs e)
		{
			long currentTime = GetCurrentTime();
			int delta = (int)(currentTime - prevTime);
			delta = delta.Clamp(0, 40);
			prevTime = currentTime;
			DoUpdate(delta);
			DoRender();
		}

		private long startTime = 0;

		private long GetCurrentTime()
		{
			long t = DateTime.Now.Ticks / 10000L;
			if (startTime == 0) {
				startTime = t;
			}
			return t - startTime;
		}

		private void DoRender()
		{
			UpdateFrameRate();
			MakeCurrent();
			app.OnRenderFrame();
			SwapBuffers();
		}

		private void DoUpdate(float delta)
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
		
		private long timeStamp;
		private int countedFrames;
		private float frameRate;

		private void UpdateFrameRate()
		{
			countedFrames++;
			long t = System.DateTime.Now.Ticks;
			long milliseconds = (t - timeStamp) / 10000;
			if (milliseconds > 1000) {
				if (timeStamp > 0)
					frameRate = (float)countedFrames / ((float)milliseconds / 1000.0f);
				timeStamp = t;
				countedFrames = 0;
			}
		}
		
		public float FrameRate { 
			get { return frameRate; } 
		}
	}
}
#endif