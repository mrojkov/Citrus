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

		public event EventHandler OnResize;

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
			Application.SupportedDeviceOrientationsChanged += ResetDeviceOrientation;
		}

		public new GameView View { get { return (GameView)base.View; } }

		public override void ViewWillTransitionToSize(CoreGraphics.CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
		{
			View.SetNeedsLayout();
		}

		internal static void ResetDeviceOrientation(DeviceOrientation supportedOrientation)
		{
			switch (supportedOrientation) {
				case DeviceOrientation.AllLandscapes:
				case DeviceOrientation.LandscapeLeft:
					SetDeviceOrientation(UIInterfaceOrientation.LandscapeLeft);
					break;
				case DeviceOrientation.LandscapeRight:
					SetDeviceOrientation(UIInterfaceOrientation.LandscapeRight);
					break;
				case DeviceOrientation.AllPortraits:
				case DeviceOrientation.Portrait:
					SetDeviceOrientation(UIInterfaceOrientation.Portrait);
					break;
				case DeviceOrientation.PortraitUpsideDown:
					SetDeviceOrientation(UIInterfaceOrientation.PortraitUpsideDown);
					break;
			}
			UIViewController.AttemptRotationToDeviceOrientation();
		}

		private static void SetDeviceOrientation(UIInterfaceOrientation orientation)
		{
			var value = new NSNumber((int)orientation);
			var key = new NSString("orientation");
			UIDevice.CurrentDevice.SetValueForKey(value, key);
		}

		private void OnAcceleration(object sender, UIAccelerometerEventArgs e)
		{
			input.NativeAcceleration = new Vector3((float)e.Acceleration.X,
				(float)e.Acceleration.Y, (float)e.Acceleration.Z);

			switch (Application.CurrentDeviceOrientation) {
				case DeviceOrientation.Portrait:
					input.Acceleration =
						new Vector3((float)e.Acceleration.X, (float)e.Acceleration.Y, (float)e.Acceleration.Z);
					break;
				case DeviceOrientation.PortraitUpsideDown:
					input.Acceleration =
						new Vector3((float)-e.Acceleration.X, (float)-e.Acceleration.Y, (float)e.Acceleration.Z);
					break;
				case DeviceOrientation.LandscapeLeft:
					input.Acceleration =
						new Vector3((float)e.Acceleration.Y, (float)-e.Acceleration.X, (float)e.Acceleration.Z);
					break;
				case DeviceOrientation.LandscapeRight:
					input.Acceleration =
						new Vector3((float)-e.Acceleration.Y, (float)e.Acceleration.X, (float)e.Acceleration.Z);
					break;
			}
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
			var screen = UIScreen.MainScreen.Bounds;
			var isOrientationDependent = UIDevice.CurrentDevice.CheckSystemVersion(8, 0) ||
				Application.CurrentDeviceOrientation == DeviceOrientation.Portrait;
			if (isOrientationDependent) {
				softKeyboard.Height = (float)(screen.Bottom - args.FrameEnd.Top);
			} else {
				switch (Application.CurrentDeviceOrientation) {
					case DeviceOrientation.PortraitUpsideDown:
						softKeyboard.Height = (float)args.FrameEnd.Bottom;
						break;
					case DeviceOrientation.LandscapeLeft:
						softKeyboard.Height = (float)(screen.Right - args.FrameEnd.Left);
						break;
					case DeviceOrientation.LandscapeRight:
						softKeyboard.Height = (float)args.FrameEnd.Right;
						break;
				}
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

		DeviceOrientation ConvertInterfaceOrientation(Vector2 size, UIInterfaceOrientation orientation)
		{
			// system orientation may not reflect real state if the game is under a window
			// which can not be rotated (for example FB login page), so prefer size over orientation.
			var isPortrait = size.Y > size.X;
			switch (orientation) {
				case UIInterfaceOrientation.LandscapeLeft:
					return isPortrait ? DeviceOrientation.Portrait : DeviceOrientation.LandscapeLeft;
				case UIInterfaceOrientation.LandscapeRight:
					return isPortrait ? DeviceOrientation.Portrait : DeviceOrientation.LandscapeRight;
				case UIInterfaceOrientation.Portrait:
					return !isPortrait ? DeviceOrientation.LandscapeLeft : DeviceOrientation.Portrait;
				case UIInterfaceOrientation.PortraitUpsideDown:
					return !isPortrait ? DeviceOrientation.LandscapeLeft : DeviceOrientation.PortraitUpsideDown;
				default:
					throw new ArgumentException("Wrong interface orientation");
			}
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);
		}

		private Vector2 prevSize;

		public override void ViewWillLayoutSubviews()
		{
			prevSize = Window.Current.ClientSize;
		}

		public override void ViewDidLayoutSubviews()
		{
			// Handle resize here (not in WillRotate) because in WillRotate we don't know
			// the resulting screen resolution.
			var toOrientation = ConvertInterfaceOrientation(Window.Current.ClientSize, InterfaceOrientation);
			var deviceRotated = toOrientation != Application.CurrentDeviceOrientation;
			Application.CurrentDeviceOrientation = toOrientation;
			// The texture stages get invalidated after device rotation. Rebind textures to fix it.
			PlatformRenderer.MarkAllTextureSlotsAsDirty();
			if (OnResize != null) {
				OnResize(this, new ResizeEventArgs { DeviceRotated = deviceRotated });
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

			public void Show(bool show)
			{
				view.ShowSoftKeyboard(show);
			}

			public bool Visible { get; internal set; }
			public float Height { get; internal set; }
			public bool Supported { get { return true; } }
		}
	}

	internal class ResizeEventArgs : EventArgs
	{
		public bool DeviceRotated;
	}
}
#endif