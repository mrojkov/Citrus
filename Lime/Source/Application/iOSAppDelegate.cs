#if iOS
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using System.Runtime.InteropServices;
using MessageUI;

namespace Lime
{
	/// <summary>
	/// The UIApplicationDelegate for the application. This class is responsible for launching the
	/// User Interface of the application, as well as listening(and optionally responding) to
	/// application events from iOS.
	/// </summary>
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		DateTime lastMemoryWarningTime;
		public static AppDelegate Instance;

		// handlers
		public delegate void AlertClickHandler(int buttonIdx);
		public delegate bool OpenURLHandler(NSUrl url);
		public delegate void ReceivedRemoteNotificationsHandler(NSDictionary userInfo);
		public delegate void RegisteredForRemoteNotificationsHandler(NSData deviceToken);
		public delegate void FailedToRegisterForRemoteNotificationsHandler(NSError error);

		public event OpenURLHandler UrlOpened;
		public event ReceivedRemoteNotificationsHandler ReceivedRemoteNotificationsHandlerEvent;
		public event RegisteredForRemoteNotificationsHandler RegisteredForRemoteNotificationsEvent;
		public event FailedToRegisterForRemoteNotificationsHandler FailedToRegisterForRemoteNotificationsEvent;
		public event Action WillTerminateEvent;
		public event Action Activated;
		public event Action Deactivated;
		public event Action LowMemory;
		// This is a mandatory property. See https://developer.xamarin.com/api/type/MonoTouch.UIKit.UIApplicationDelegate/
		public override UIWindow Window { get; set; }

		public AppDelegate()
		{
			Instance = this;
			AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
		{
			// Grisha: UIInterfaceOrientationMask.Portrait is required by GameCenter for iPhone in iOS 6.
			// so, if your app works in Landscape mode, use AllButUpsideDown.
			// read
			// http://stackoverflow.com/questions/12488838/game-center-login-lock-in-landscape-only-in-i-os-6
			// for more information.
			return UIInterfaceOrientationMask.All;
		}

		UIAlertView pleaseRateMessage = null;
		public void ShowAlertMessage(string title, string text, string cancelTitle, string[] otherButtons, AlertClickHandler onClick)
		{
			if (pleaseRateMessage == null) {
				pleaseRateMessage = new UIAlertView(title, text, null, cancelTitle, otherButtons);
				pleaseRateMessage.Show();
				pleaseRateMessage.Clicked += (sender, buttonArgs) => {
					onClick((int)buttonArgs.ButtonIndex);
					pleaseRateMessage = null;
				};
			}
		}

		public override void ReceiveMemoryWarning(UIApplication application)
		{
			if ((DateTime.UtcNow - lastMemoryWarningTime).TotalSeconds < 15) {
				return;
			}
			lastMemoryWarningTime = DateTime.UtcNow;
			Logger.Write("Memory warning, texture memory: {0}mb", CommonTexture.TotalMemoryUsedMb);
			Lime.TexturePool.Instance.DiscardTexturesUnderPressure();
			LowMemory?.Invoke();
			System.GC.Collect();
		}

		static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Write("========================= CRASH REPORT ============================\n" + e.ExceptionObject.ToString());
		}

		//iOS9 changed the function signature for OpenURL. This is the new signature. Fixes [WSAG-3193].
		public override bool OpenUrl (UIApplication application, NSUrl url, NSDictionary options)
		{
			return InvokeUrlOpenedDelegate(url);
		}

		public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			return InvokeUrlOpenedDelegate(url);
		}

		bool InvokeUrlOpenedDelegate(NSUrl url)
		{
			if (UrlOpened != null) {
				bool result = false;
				foreach (OpenURLHandler d in UrlOpened.GetInvocationList()) {
					result = result || d(url);
				}
				return result;
			}
			return false;
		}

		public override void OnActivated(UIApplication application)
		{
			if (Activated != null) {
				Activated();
			}
		}

		public override void WillTerminate(UIApplication application)
		{
			if (WillTerminateEvent != null) {
				WillTerminateEvent();
			}
		}

		public override void OnResignActivation(UIApplication application)
		{
			if (Deactivated != null) {
				Deactivated();
			}
		}

		public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
		{
			if (ReceivedRemoteNotificationsHandlerEvent != null) {
				ReceivedRemoteNotificationsHandlerEvent(userInfo);
			}
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			if (RegisteredForRemoteNotificationsEvent != null) {
				RegisteredForRemoteNotificationsEvent(deviceToken);
			}
		}

		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
			if (FailedToRegisterForRemoteNotificationsEvent != null) {
				FailedToRegisterForRemoteNotificationsEvent(error);
			}
		}
	}
}
#endif
