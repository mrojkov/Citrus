#if iOS
using System;

using Foundation;
using UIKit;

namespace Lime
{
	public class GameController : UIViewController
	{
		private NSObject keyboardShowNotification;
		private NSObject keyboardHideNotification;
		private NSObject keyboardWillChangeFrameNotification;
		private NSObject keyboardDidChangeFrameNotification;
		private SoftKeyboard softKeyboard;
		private Input input;

		public event Action ViewDidLayoutSubviewsEvent;
		public bool SoftKeyboardBeingShownOrHid { get; private set; }
		public bool LockDeviceOrientation { get; set; }

		public GameController(Input input) : base()
		{
			this.input = input;
			base.View = new GameView(input);
			softKeyboard = new SoftKeyboard(View);
			Application.SoftKeyboard = softKeyboard;
			UIAccelerometer.SharedAccelerometer.UpdateInterval = 0.05;
			UIAccelerometer.SharedAccelerometer.Acceleration += OnAcceleration;
			Application.CurrentDeviceOrientation = ConvertInterfaceOrientation(InterfaceOrientation);
		}

		public new GameView View { get { return (GameView)base.View; } }

		private void OnAcceleration(object sender, UIAccelerometerEventArgs e)
		{
			input.Acceleration = new Vector3((float)e.Acceleration.X,
				(float)e.Acceleration.Y, (float)e.Acceleration.Z);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			keyboardHideNotification = UIKeyboard.Notifications.ObserveDidHide(KeyboardHideCallback);
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
			if (keyboardHideNotification != null) {
				keyboardHideNotification.Dispose();
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
			SoftKeyboardBeingShownOrHid = true;
			var beginFrame = args.FrameBegin;
			var screenRect = UIScreen.MainScreen.Bounds;
			if (!beginFrame.IntersectsWith(screenRect)) {
				SoftKeyboardBeingShownOrHid = false;
			}
		}

		private void KeyboardDidChangeFrameCallback(object sender, UIKeyboardEventArgs args)
		{
			SoftKeyboardBeingShownOrHid = false;
			var endFrame = args.FrameEnd;
			var screenRect = UIScreen.MainScreen.Bounds;
			if (!endFrame.IntersectsWith(screenRect)) {
				softKeyboard.RaiseHidden();
			}
		}

		private void KeyboardShowCallback(object sender, UIKeyboardEventArgs args)
		{
			softKeyboard.Visible = true;
			var scale = UIScreen.MainScreen.Scale;

			// iPad 2 return keyboard height in Height, but iPad 3 return keyboard height in Width.
			// So, trying to determine where the real height is. 
			var rectEnd = args.FrameEnd;
			var rectBegin = args.FrameBegin;
			rectBegin.X  = rectBegin.X < 0? 0 : rectBegin.X;
			rectEnd.X = rectEnd.X < 0 ? 0 : rectEnd.X;
			if (rectEnd.X == 0 && rectBegin.X == 0 && rectEnd.Height < rectEnd.Width) {
				softKeyboard.Height = (float)(rectEnd.Height * scale);
			} else {
				softKeyboard.Height = (float)(rectEnd.Width * scale);
			}
		}

		private void KeyboardHideCallback(object sender, UIKeyboardEventArgs args)
		{
			softKeyboard.Height = 0;
			softKeyboard.Visible = false;
		}
		 
		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
			if (LockDeviceOrientation && Application.CurrentDeviceOrientation != 0) {
				return ConvertDeviceOrientationToInterfaceOrientationMask(Application.CurrentDeviceOrientation);
			}
			UIInterfaceOrientationMask mask = 0;
			if ((Application.SupportedDeviceOrientations & DeviceOrientation.LandscapeLeft) != 0)
				mask = mask | UIInterfaceOrientationMask.LandscapeLeft;
			if ((Application.SupportedDeviceOrientations & DeviceOrientation.LandscapeRight) != 0)
				mask = mask | UIInterfaceOrientationMask.LandscapeRight;
			if ((Application.SupportedDeviceOrientations & DeviceOrientation.Portrait) != 0)
				mask = mask | UIInterfaceOrientationMask.Portrait;
			if ((Application.SupportedDeviceOrientations & DeviceOrientation.PortraitUpsideDown) != 0)
				mask = mask | UIInterfaceOrientationMask.PortraitUpsideDown;
			return mask;
		}

		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
			if (LockDeviceOrientation) {
				return false;
			}
			switch (toInterfaceOrientation) {
			case UIInterfaceOrientation.LandscapeLeft:
				return (Application.SupportedDeviceOrientations & DeviceOrientation.LandscapeLeft) != 0;
			case UIInterfaceOrientation.LandscapeRight:
				return (Application.SupportedDeviceOrientations & DeviceOrientation.LandscapeRight) != 0;
			case UIInterfaceOrientation.Portrait:
				return (Application.SupportedDeviceOrientations & DeviceOrientation.Portrait) != 0;
			case UIInterfaceOrientation.PortraitUpsideDown:
				return (Application.SupportedDeviceOrientations & DeviceOrientation.PortraitUpsideDown) != 0;
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
			if (toOrientation != Application.CurrentDeviceOrientation) {
				Application.CurrentDeviceOrientation = toOrientation;
				// Handle resize here (not in WillRotate) because in WillRotate we don't know
				// the resulting screen resolution.
				if (ViewDidLayoutSubviewsEvent != null) {
					ViewDidLayoutSubviewsEvent();
				}
			}
		}

		private class SoftKeyboard : ISoftKeyboard
		{
			GameView view;

			public event Action Shown;
			public event Action Hidden;

			public SoftKeyboard(GameView view)
			{
				this.view = view;
			}

			internal void RaiseHidden()
			{
				if (Hidden != null) {
					Hidden();
				}
			}

			public void Show(bool show, string text)
			{
				view.ShowSoftKeyboard(show, text);
			}

			public void ChangeText(string text)
			{
				view.ChangeSoftKeyboardText(text);
			}

			public bool Visible { get; internal set; }
			public float Height { get; internal set; }
			public bool Supported { get { return true; } }
		}
	}
}
#endif