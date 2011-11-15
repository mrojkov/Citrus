#if iOS
using System;
using OpenTK;
using OpenTK.Graphics.ES20;
using GL1 = OpenTK.Graphics.ES11.GL;
using All1 = OpenTK.Graphics.ES11.All;
using OpenTK.Platform.iPhoneOS;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

namespace Lime
{
	internal class GameView : iPhoneOSGameView
	{
		public GameView () : base (new RectangleF (0, 0, 0, 0))
		{
			LayerRetainsBacking = false;
			LayerColorFormat = EAGLColorFormat.RGB565;
		}

		[Export ("layerClass")]
		public static new Class GetLayerClass ()
		{
			return iPhoneOSGameView.GetLayerClass ();
		}

		protected override void ConfigureLayer (CAEAGLLayer eaglLayer)
		{
			eaglLayer.Opaque = true;
		}

		protected override void CreateFrameBuffer ()
		{
			ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
			base.CreateFrameBuffer ();
		}

		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			double delta = e.Time; 
			// Here is protection against time leap on inactive state and low FPS
			if (delta > 0.5)
				delta = 0.01;
			else if (delta > 0.1)
				delta = 0.1;
			Application.gameApp.OnUpdateFrame (delta);
		}

		protected override void OnRenderFrame (FrameEventArgs e)
		{
			MakeCurrent ();
			Application.gameApp.OnRenderFrame ();
			SwapBuffers ();
			UpdateFrameRate ();
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