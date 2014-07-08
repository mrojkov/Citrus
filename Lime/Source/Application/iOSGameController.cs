#if iOS
using System;
using OpenTK;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace Lime
{
	public class GameController : UIViewController
	{
		public static GameController Instance;

		public GameController() : base()
		{
			Instance = this;
			base.View = new GameView();
			UIAccelerometer.SharedAccelerometer.UpdateInterval = 0.05;
			UIAccelerometer.SharedAccelerometer.Acceleration += OnAcceleration;
			Application.Instance.CurrentDeviceOrientation = ConvertInterfaceOrientation(InterfaceOrientation);
		}

		public new GameView View { get { return (GameView)base.View; } }

		private void OnAcceleration(object sender, UIAccelerometerEventArgs e)
		{
			Input.Acceleration = new Vector3((float)e.Acceleration.X,
				(float)e.Acceleration.Y, (float)e.Acceleration.Z);
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			UIInterfaceOrientationMask mask = 0;
			if ((Application.Instance.GetSupportedDeviceOrientations() & DeviceOrientation.LandscapeLeft) != 0)
				mask = mask | UIInterfaceOrientationMask.LandscapeLeft;
			if ((Application.Instance.GetSupportedDeviceOrientations() & DeviceOrientation.LandscapeRight) != 0)
				mask = mask | UIInterfaceOrientationMask.LandscapeRight;
			if ((Application.Instance.GetSupportedDeviceOrientations() & DeviceOrientation.Portrait) != 0)
				mask = mask | UIInterfaceOrientationMask.Portrait;
			if ((Application.Instance.GetSupportedDeviceOrientations() & DeviceOrientation.PortraitUpsideDown) != 0)
				mask = mask | UIInterfaceOrientationMask.PortraitUpsideDown;
			return mask;
		}

		[Obsolete]
		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
			switch (toInterfaceOrientation) {
			case UIInterfaceOrientation.LandscapeLeft:
				return (Application.Instance.GetSupportedDeviceOrientations() & DeviceOrientation.LandscapeLeft) != 0;
			case UIInterfaceOrientation.LandscapeRight:
				return (Application.Instance.GetSupportedDeviceOrientations() & DeviceOrientation.LandscapeRight) != 0;
			case UIInterfaceOrientation.Portrait:
				return (Application.Instance.GetSupportedDeviceOrientations() & DeviceOrientation.Portrait) != 0;
			case UIInterfaceOrientation.PortraitUpsideDown:
				return (Application.Instance.GetSupportedDeviceOrientations() & DeviceOrientation.PortraitUpsideDown) != 0;
			}
			return false;
		}

		DeviceOrientation ConvertInterfaceOrientation(UIInterfaceOrientation orientation)
		{
			switch (orientation) {
			case UIInterfaceOrientation.LandscapeLeft:
				return DeviceOrientation.LandscapeLeft;
			case UIInterfaceOrientation.LandscapeRight:
				return DeviceOrientation.LandscapeRight;
			case UIInterfaceOrientation.Portrait:
				return DeviceOrientation.Portrait;
			case UIInterfaceOrientation.PortraitUpsideDown:
				return DeviceOrientation.PortraitUpsideDown;
			default:
				throw new ArgumentException("Wrong interface orientation");
			}
		}

		bool rotating;

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			rotating = true;
			base.WillRotate(toInterfaceOrientation, duration);
		}

		public override void ViewDidLayoutSubviews()
		{
			var toOrientation = ConvertInterfaceOrientation(this.InterfaceOrientation);
			Application.Instance.CurrentDeviceOrientation = toOrientation;
			base.ViewDidLayoutSubviews();
			if (rotating) {
				rotating = false;
				// OnDeviceRotate() called from here (not in WillRotate) because in WillRotate we don't know
				// the resulting screen resolution.
				Application.Instance.OnDeviceRotate();
			}
		}
	}
}
#endif