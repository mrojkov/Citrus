#if MAC
using System;
using System.Drawing;
using MonoMac.OpenGL;
using Lime;
using MonoMac.AppKit;

namespace Lime
{
	public class GameView : MonoMac.OpenGL.MonoMacGameView
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

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			float delta;
			RefreshFrameTimeStamp(out delta);
			Input.ProcessPendingKeyEvents();
			Application.Instance.OnUpdateFrame(delta);
			AudioSystem.Update();

			var p = Window.MouseLocationOutsideOfEventStream;
			Input.MousePosition = new Vector2(p.X, Size.Height - p.Y);

			Input.TextInput = null;
			Input.CopyKeysState();
			if (DidUpdated != null) {
				DidUpdated();
			}
		}

		private DateTime lastFrameTimeStamp = DateTime.UtcNow;
		private void RefreshFrameTimeStamp(out float delta)
		{
			var now = DateTime.UtcNow;
			delta = (float)(now - lastFrameTimeStamp).TotalSeconds;
			delta = delta.Clamp(0, 1 / Application.LowFPSLimit);
			lastFrameTimeStamp = now;
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