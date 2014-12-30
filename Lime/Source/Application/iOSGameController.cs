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
		private NSObject keyboardShowNotification;
		private NSObject keyboardWillChangeFrameNotification;
		private NSObject keyboardDidChangeFrameNotification;

		public float KeyboardHeight { get; private set; }
		public bool IsKeyboardChanging { get; private set; }

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

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			keyboardShowNotification = UIKeyboard.Notifications.ObserveWillShow(KeyboardShowCallback);
			keyboardWillChangeFrameNotification = UIKeyboard.Notifications.ObserveWillChangeFrame(KeyboardWillChangeFrameCallback);
			keyboardDidChangeFrameNotification = UIKeyboard.Notifications.ObserveDidChangeFrame(KeyboardDidChangeFrameCallback);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			if (keyboardShowNotification != null) {
				keyboardShowNotification.Dispose();
			}
			if (keyboardWillChangeFrameNotification != null) {
				keyboardWillChangeFrameNotification.Dispose();
			}
			if (keyboardDidChangeFrameNotification != null) {
				keyboardDidChangeFrameNotification.Dispose();
			}
		}

		private void KeyboardWillChangeFrameCallback(object sender, UIKeyboardEventArgs args)
		{
			IsKeyboardChanging = true;
			var beginFrame = args.FrameBegin;
			var screenRect = UIScreen.MainScreen.Bounds;
			if (!beginFrame.IntersectsWith(screenRect)) {
				IsKeyboardChanging = false;
			}
		}

		private void KeyboardDidChangeFrameCallback(object sender, UIKeyboardEventArgs args)
		{
			IsKeyboardChanging = false;
			var endFrame = args.FrameEnd;
			var screenRect = UIScreen.MainScreen.Bounds;
			if (!endFrame.IntersectsWith(screenRect)) {
				Application.Instance.OnScreenKeyboardHide();
			}		
		}

		private void KeyboardShowCallback(object sender, UIKeyboardEventArgs args)
		{
			var scale = UIScreen.MainScreen.Scale;

			// iPad 2 return keyboard height in Height, but iPad 3 return keyboard height in Width.
			// So, trying to determine where the real height is. 
			var rectEnd = args.FrameEnd;
			var rectBegin = args.FrameBegin;
			if (rectEnd.X == 0 && rectBegin.X == 0) {
				KeyboardHeight = rectEnd.Height * scale;
			} else {
				KeyboardHeight = rectEnd.Width * scale;
			}
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			UIInterfaceOrientationMask mask = 0;
			if (!Application.Instance.Active && Application.Instance.CurrentDeviceOrientation != 0) {
				return mask | ConvertDeviceOrientationToInterfaceOrientationMask(Application.Instance.CurrentDeviceOrientation);
			}
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
			if (!Application.Instance.Active) {
				return false;
			}
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

		UIInterfaceOrientationMask ConvertDeviceOrientationToInterfaceOrientationMask(DeviceOrientation orientation)
		{
			switch (orientation) {
			case DeviceOrientation.LandscapeLeft:
				return UIInterfaceOrientationMask.LandscapeLeft;
			case DeviceOrientation.LandscapeRight:
				return UIInterfaceOrientationMask.LandscapeRight;
			case DeviceOrientation.Portrait:
				return UIInterfaceOrientationMask.Portrait;
			case DeviceOrientation.PortraitUpsideDown:
				return UIInterfaceOrientationMask.PortraitUpsideDown;
			default:
				throw new ArgumentException("Wrong interface orientation");
			}
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

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
		}

		public override void ViewDidLayoutSubviews()
		{
			var toOrientation = ConvertInterfaceOrientation(this.InterfaceOrientation);
			if (toOrientation != Application.Instance.CurrentDeviceOrientation) {
				Application.Instance.CurrentDeviceOrientation = toOrientation;
				// OnDeviceRotate() called from here (not in WillRotate) because in WillRotate we don't know
				// the resulting screen resolution.
				Application.Instance.OnDeviceRotate();
			}
		}
	}
}
#endif