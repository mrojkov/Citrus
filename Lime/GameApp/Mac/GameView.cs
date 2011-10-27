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
		GameApp game;
		
		public GameView (RectangleF frame, NSOpenGLContext context, GameApp game) : base (frame, context)
		{
			this.game = game;
			AutoresizingMask = NSViewResizingMask.HeightSizable
				| NSViewResizingMask.MaxXMargin 
				| NSViewResizingMask.MinYMargin
				| NSViewResizingMask.WidthSizable;
		}
		
		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			double delta = e.Time; 
			// Here is protection against time leap on inactive state and low FPS
			if (delta > 0.5)
				delta = 0.01;
			else if (delta > 0.1)
				delta = 0.1;
			game.OnUpdateFrame (delta);
		}
		
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			UpdateFrameRate ();
			UpdateView ();
			game.OnRenderFrame ();
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
			get {
				return frameRate;
			}
		}
	}
}
#endif