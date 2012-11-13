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

		UITouch[] activeTouches = new UITouch[Input.MaxTouches];

		public GameController() : base()
		{
			Instance = this;
			base.View = new GameView();
			UIAccelerometer.SharedAccelerometer.UpdateInterval = 0.05;
			UIAccelerometer.SharedAccelerometer.Acceleration += OnAcceleration;
		}

		public new GameView View { get { return (GameView)base.View; } }

		private void OnAcceleration(object sender, UIAccelerometerEventArgs e)
		{
			Input.Acceleration = new Vector3((float)e.Acceleration.X,
				(float)e.Acceleration.Y, (float)e.Acceleration.Z);
		}

		public void Activate()
		{
			View.Run();
			Application.Instance.OnActivate();
		}

		public void Deactivate()
		{
			// Send signal of dectivation to gamelogic and do last update & render.
			// So, game may show pause dialog here.
			Application.Instance.OnDeactivate();
			View.UpdateFrame();
			View.RenderFrame();
			View.Stop();
		}

		public static System.Drawing.PointF GetTouchLocationInView(UITouch touch, UIView view)
		{
			// This code absolute equivalent to:
			//		return touch.LocationInView(this.View);
			// but later line causes crash when being run under XCode,
			// so we managed this workaround:
			System.Drawing.PointF result;
			var selector = Selector.GetHandle("locationInView:");
			if (MonoTouch.ObjCRuntime.Runtime.Arch == Arch.DEVICE)
			{
				Messaging.PointF_objc_msgSend_stret_IntPtr(out result, touch.Handle, selector, (view != null) ? view.Handle : IntPtr.Zero);
			}
			else
			{
				result = Messaging.PointF_objc_msgSend_IntPtr(touch.Handle, selector, (view != null) ? view.Handle : IntPtr.Zero);
			}
			return result;
		}
		
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			foreach (var touch in touches.ToArray<UITouch>()) {
				for (int i = 0; i < Input.MaxTouches; i++) {
					if (activeTouches[i] == null) {
						var pt = GetTouchLocationInView(touch, this.View);
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
						var pt = GetTouchLocationInView(touch, this.View);
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
						var pt = GetTouchLocationInView(touch, this.View);
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

		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
			switch (toInterfaceOrientation)
			{
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

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			Application.Instance.OnDeviceRotated();
		}
	}
}
#endif