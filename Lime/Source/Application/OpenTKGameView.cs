#if WIN
using System;
using OpenTK;
using OpenTK.Input;

namespace Lime
{
	public class GameView : OpenTK.GameWindow, IGameWindow
	{
		GameApp app;

		public GameView(GameApp app)
			: base(640, 480, new OpenTK.Graphics.GraphicsMode(32, 0, 0, 4))
		{
			this.app = app;
			AudioSystem.Initialize();
			app.OnCreate(this);
			//this.Keyboard.KeyDown += HandleKeyDown;
			//this.Keyboard.KeyUp += HandleKeyUp;
			//this.KeyPress += HandleKeyPress;
			//this.Mouse.ButtonDown += HandleMouseButtonDown;
			//this.Mouse.ButtonUp += HandleMouseButtonUp;
			this.Mouse.Move += HandleMouseMove;
			this.Location = new System.Drawing.Point(100, 100);
		}
/*
		void HandleKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			if (e.Key == OpenTK.Input.Key.Escape)
				this.Exit();
			if (e.Key == OpenTK.Input.Key.F12) {
				FullScreen = !FullScreen;
			}
			app.OnKeyDown((Key)e.Key);
		}

		void HandleKeyUp(object sender, KeyboardKeyEventArgs e)
		{
			app.OnKeyUp((Key)e.Key);
		}

		void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			app.OnKeyPress(e.KeyChar);
		}

		void HandleMouseButtonUp(object sender, MouseButtonEventArgs e)
		{
			Vector2 position = new Vector2(e.X, e.Y) * Input.ScreenToWorldTransform;
			switch(e.Button) {
			case OpenTK.Input.MouseButton.Left:
				app.OnMouseUp(MouseButton.Left, position);
				break;
			case OpenTK.Input.MouseButton.Right:
				app.OnMouseUp(MouseButton.Right, position);
				break;
			}
		}

		void HandleMouseButtonDown(object sender, MouseButtonEventArgs e)
		{
			Vector2 position = new Vector2(e.X, e.Y) * Input.ScreenToWorldTransform;
			switch(e.Button) {
			case OpenTK.Input.MouseButton.Left:
				app.OnMouseDown(MouseButton.Left, position);
				break;
			case OpenTK.Input.MouseButton.Right:
				app.OnMouseDown(MouseButton.Right, position);
				break;
			}
		}
*/
		void HandleMouseMove(object sender, MouseMoveEventArgs e)
		{
			Vector2 position = new Vector2(e.X, e.Y) * Input.ScreenToWorldTransform;
			Input.MousePosition = position;
		}

		protected override void OnClosed(EventArgs e)
		{
			TexturePool.Instance.DiscardAllTextures();
			AudioSystem.Terminate();
		}

		protected override void OnUpdateFrame(OpenTK.FrameEventArgs e)
		{
			Input.Update();
			double delta = e.Time;
			// Here is protection against time leap on inactive state and low FPS
			if (delta > 0.5)
				delta = 0.01;
			else if (delta > 0.1)
				delta = 0.1;
			app.OnUpdateFrame(delta);
		}
		
		protected override void OnRenderFrame(OpenTK.FrameEventArgs e)
		{
			UpdateFrameRate();
			MakeCurrent();
			app.OnRenderFrame();
			SwapBuffers();
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
		
		public DeviceOrientation CurrentDeviceOrientation { 
			get { return DeviceOrientation.LandscapeLeft; } 
		}

	}
}
#endif