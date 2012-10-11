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
			: base(640, 480, new OpenTK.Graphics.GraphicsMode(32, 0, 0, 4))
		{
			Instance = this;
			this.app = app;
			app.Active = true;
			AudioSystem.Initialize(16, args);
			app.OnCreate();
			this.Keyboard.KeyDown += HandleKeyDown;
			this.Keyboard.KeyUp += HandleKeyUp;
			this.KeyPress += HandleKeyPress;
			this.Mouse.ButtonDown += HandleMouseButtonDown;
			this.Mouse.ButtonUp += HandleMouseButtonUp;
			this.Mouse.Move += HandleMouseMove;
			this.Location = new System.Drawing.Point(0, 0);
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

		// Why several OnUpdateFrame() calls before OnRenderFrame()?
		protected override void OnUpdateFrame(OpenTK.FrameEventArgs e)
		{
		}

		private long updateTime;
		private long updateDelta;
		private int renderCycle;

		protected override void OnRenderFrame(OpenTK.FrameEventArgs e)
		{
			long time = GetTime();
			// !!! Just for battery saving sake
			// System.Threading.Thread.Sleep(10);
			if ((renderCycle++ % 4) == 0) { //time - updateTime > 20) {
				updateDelta = time - updateTime;
				updateTime = time;
				DoUpdate(updateDelta);
			}
			float extrapolation = 0;
			if (updateDelta > 0) {
				extrapolation = (float)(time - updateTime) / (float)updateDelta;
			}
			DoRender(extrapolation);
		}

		private long startTime = 0;

		private long GetTime()
		{
			long t = DateTime.Now.Ticks / 10000L;
			if (startTime == 0) {
				startTime = t;
			}
			return t - startTime;
		}

		//private long RefreshDelta()
		//{
		//	long delta = (System.DateTime.Now.Ticks / 10000L) - tickCount;
		//	delta = Math.Max(0, delta); // How can it be possible?
		//	if (tickCount == 0) {
		//		tickCount = delta;
		//		delta = 0;
		//	} else {
		//		tickCount += delta;
		//	}
		//	// Ensure time delta lower bound is 16.6 frames per second.
		//	// This is protection against time leap on inactive state
		//	// and multiple updates of node hierarchy.
		//	delta = Math.Min(delta, 60);
		//	return delta;
		//}

		private void DoRender(float extrapolation)
		{
			UpdateFrameRate();
			MakeCurrent();
			app.OnRenderFrame(extrapolation);
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