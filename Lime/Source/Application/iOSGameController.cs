#if iOS
using System;
using OpenTK;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

namespace Lime
{
	internal class GameController : UIViewController, IGameWindow
	{
		public GameController() : base()
		{
			base.View = new GameView();
			UIAccelerometer.SharedAccelerometer.UpdateInterval = 0.05;
			UIAccelerometer.SharedAccelerometer.Acceleration += OnAcceleration;
		}

		new GameView View { get { return (GameView)base.View; } }

		bool IGameWindow.FullScreen {
			get { return true; }
			set {}
		}

		Size IGameWindow.WindowSize {
			get {
				return new Size(View.Size.Width, View.Size.Height);
			}
			set {}
		}
		
		private void OnAcceleration (object sender, UIAccelerometerEventArgs e)
		{
			Input.Acceleration = new Vector3((float)e.Acceleration.X, (float)e.Acceleration.Y, (float)e.Acceleration.Z);
		}

		public void Activate()
		{
			View.Run();
		}

		public void Deactivate()
		{
			View.Stop();
		}
		
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			var pt = (touches.AnyObject as UITouch).LocationInView(this.View);
			Vector2 position = new Vector2(pt.X, pt.Y) * Input.ScreenToWorldTransform;
			Input.MousePosition = position;
			Input.Update(true);
		}
		
		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			var pt = (touches.AnyObject as UITouch).LocationInView(this.View);
			Vector2 position = new Vector2(pt.X, pt.Y) * Input.ScreenToWorldTransform;
			Input.MousePosition = position;
		}
		
		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			var pt = (touches.AnyObject as UITouch).LocationInView(this.View);
			Vector2 position = new Vector2(pt.X, pt.Y) * Input.ScreenToWorldTransform;
			Input.MousePosition = position;
			Input.Update(false);
		}

		public DeviceOrientation CurrentDeviceOrientation {
			get {
				switch(InterfaceOrientation) {
				case UIInterfaceOrientation.Portrait:
					return DeviceOrientation.Portrait;
				case UIInterfaceOrientation.PortraitUpsideDown:
					return DeviceOrientation.PortraitUpsideDown;
				case UIInterfaceOrientation.LandscapeLeft:
					return DeviceOrientation.LandscapeLeft;
				case UIInterfaceOrientation.LandscapeRight:
				default:
					return DeviceOrientation.LandscapeRight;
				}
			}
		}

		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
			switch(toInterfaceOrientation)
			{
			case UIInterfaceOrientation.LandscapeLeft:
				return (Application.gameApp.GetSupportedDeviceOrientations() & DeviceOrientation.LandscapeLeft) != 0;
			case UIInterfaceOrientation.LandscapeRight:
				return (Application.gameApp.GetSupportedDeviceOrientations() & DeviceOrientation.LandscapeRight) != 0;
			case UIInterfaceOrientation.Portrait:
				return (Application.gameApp.GetSupportedDeviceOrientations() & DeviceOrientation.Portrait) != 0;
			case UIInterfaceOrientation.PortraitUpsideDown:
				return (Application.gameApp.GetSupportedDeviceOrientations() & DeviceOrientation.PortraitUpsideDown) != 0;
			}
			return false;
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			Application.gameApp.OnDeviceRotated(CurrentDeviceOrientation);
		}

		public float FrameRate {
			get {
				return View.FrameRate;
			}
		}
	}
}
#endif