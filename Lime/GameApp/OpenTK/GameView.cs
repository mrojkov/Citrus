#if WIN
using System;
using OpenTK;
using OpenTK.Input;

namespace Lime
{
	public class GameView : OpenTK.GameWindow, IGameWindow
	{
		IGameApp app;
		
		public GameView (IGameApp app)
			: base (800, 600, new OpenTK.Graphics.GraphicsMode (32, 0, 0, 4))
		{
			this.app = app;
			app.OnCreate (this);
			Keyboard.KeyDown += HandleKeyboardKeyDown;
		}
		
		protected override void OnClosed (EventArgs e)
		{
			TexturePool.Instance.DiscardAll ();
		}

		void HandleKeyboardKeyDown (object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				this.Exit ();
			if (e.Key == Key.F12) {
				FullScreen = !FullScreen;
			}			
		}
		
		protected override void OnUpdateFrame (OpenTK.FrameEventArgs e)
		{
			double delta = e.Time;
			// Here is protection against time leap on inactive state and low FPS
			if (delta > 0.5)
				delta = 0.01;
			else if (delta > 0.1)
				delta = 0.1;
			app.OnUpdateFrame (0.1);
		}
		
		protected override void OnRenderFrame (OpenTK.FrameEventArgs e)
		{
			UpdateFrameRate ();
			MakeCurrent ();
			app.OnRenderFrame ();
			SwapBuffers ();
		}
		
		public Size WindowSize { 
			get { return new Size (ClientSize.Width, ClientSize.Height); } 
			set { this.ClientSize = new System.Drawing.Size (value.Width, value.Height); } 
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

		private void UpdateFrameRate ()
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