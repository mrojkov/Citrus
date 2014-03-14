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
		internal static Action DidUpdated;

		public readonly RenderingApi RenderingApi = RenderingApi.OpenGL;

		public GameView(RectangleF frame, NSOpenGLContext context) : base(frame, context)
		{
			Instance = this;
			AutoresizingMask = NSViewResizingMask.HeightSizable
				| NSViewResizingMask.MaxXMargin 
				| NSViewResizingMask.MinYMargin
				| NSViewResizingMask.WidthSizable;
		}

		private long tickCount;

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			long delta = (System.DateTime.Now.Ticks / 10000) - tickCount;
			if (tickCount == 0) {
				tickCount = delta;
				delta = 0;
			} else {
				tickCount += delta;
			}
			Input.ProcessPendingKeyEvents();
			Input.MouseVisible = true;
			// Ensure time delta lower bound is 16.6 frames per second.
			// This is protection against time leap on inactive state
			// and multiple updates of node hierarchy.
 			delta = Math.Min(delta, 60);
			Application.Instance.OnUpdateFrame((int)delta);
            AudioSystem.Update();
			Input.TextInput = null;
			Input.CopyKeysState();
			if (DidUpdated != null) {
				DidUpdated();
			}
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