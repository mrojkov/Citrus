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
	internal class GameController : UIViewController, IGameWindow
	{
		UITouch[] activeTouches = new UITouch[Input.MaxTouches];
			
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
			foreach (var touch in touches.ToArray<UITouch>()) {
				for (int i = 0; i < Input.MaxTouches; i++) {
					if (activeTouches[i] == null) {
						var pt = touch.LocationInView(this.View);
						Vector2 position = new Vector2(pt.X, pt.Y) * Input.ScreenToWorldTransform;
						if (i == 0) {
							Input.MousePosition = position;
							Input.SetKeyState(Key.Mouse0, true);
						}
						Key key = (Key)((int)Key.Touch0 + i);
						Input.SetTouchPosition(i, position);
						activeTouches[i] = touch;
						Input.SetKeyState(key, true);
						break;
					}
				}
			}
		}
		
		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			foreach (var touch in touches.ToArray<UITouch>()) {
				for (int i = 0; i < Input.MaxTouches; i++) {
					if (activeTouches[i] == touch) {
						var pt = touch.LocationInView(this.View);
						Vector2 position = new Vector2(pt.X, pt.Y) * Input.ScreenToWorldTransform;
						if (i == 0) {
							Input.MousePosition = position;
						}
						Input.SetTouchPosition(i, position);
					}
				}
			}
		}
		
		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			foreach (var touch in touches.ToArray<UITouch>()) {
				for (int i = 0; i < Input.MaxTouches; i++) {
					if (activeTouches[i] == touch) {
						var pt = touch.LocationInView(this.View);
						Vector2 position = new Vector2(pt.X, pt.Y) * Input.ScreenToWorldTransform;
						if (i == 0) {
							Input.MousePosition = position;
							Input.SetKeyState(Key.Mouse0, false);
						}
						activeTouches[i] = null;
						Key key = (Key)((int)Key.Touch0 + i);
						Input.SetTouchPosition(i, position);
						//Input.ScheduleKeyupEvent(key);
						Input.SetKeyState(key, false);
					}
				}
			}
		}
		
		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			TouchesEnded(touches, evt);
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