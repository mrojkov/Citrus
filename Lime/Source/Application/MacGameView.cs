#if MAC
using System;
using System.Drawing;
using MonoMac.OpenGL;
using Lime;
using MonoMac.AppKit;

namespace Lime
{
	internal class GameView : MonoMac.OpenGL.MonoMacGameView
	{
		public static GameView Instance;

		public GameView(RectangleF frame, NSOpenGLContext context) : base(frame, context)
		{
			Instance = this;
			AutoresizingMask = NSViewResizingMask.HeightSizable
				| NSViewResizingMask.MaxXMargin 
				| NSViewResizingMask.MinYMargin
				| NSViewResizingMask.WidthSizable;
		}
		
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			double delta = e.Time; 
			// Here is protection against time leap on inactive state and low FPS
			if (delta > 0.5)
				delta = 0.01;
			else if (delta > 0.1)
				delta = 0.1;
			Application.Instance.OnUpdateFrame(delta);
		}
		
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			UpdateFrameRate();
			UpdateView();
			Application.Instance.OnRenderFrame();
		}

		public override void MouseDown(NSEvent theEvent)
		{
			var p = theEvent.LocationInWindow;
			Input.MousePosition = new Vector2(p.X, Size.Height - p.Y);
			Input.SetKeyState(Key.Mouse0, true);
			base.MouseDown(theEvent);
		}
		
		public override void MouseUp(NSEvent theEvent)
		{
			var p = theEvent.LocationInWindow;
			Input.MousePosition = new Vector2(p.X, Size.Height - p.Y);
			Input.SetKeyState(Key.Mouse0, false);
			base.MouseUp(theEvent);
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
			get {
				return frameRate;
			}
		}
	}
}
#endif