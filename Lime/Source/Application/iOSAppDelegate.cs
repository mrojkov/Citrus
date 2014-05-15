#if iOS
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Runtime.InteropServices;
using MonoTouch.MessageUI;

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
		public static AppDelegate Instance;

		public NSDictionary LaunchOptions;

		// handlers
		public delegate void AlertClickHandler(int buttonIdx);
		public delegate bool OpenURLHandler(NSUrl url);
		public delegate void RegisteredForRemoteNotificationsHandler(NSData deviceToken);

		public event OpenURLHandler UrlOpened;
		public event RegisteredForRemoteNotificationsHandler RegisteredForRemoteNotificationsEvent;
		public event Action WillTerminateEvent;

		// class-level declarations
        public override UIWindow Window { get; set; }
		public GameController GameController { get; private set; }

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
					onClick(buttonArgs.ButtonIndex);
					pleaseRateMessage = null;
				};
			}
		}

		public override void ReceiveMemoryWarning(UIApplication application)
		{
			Logger.Write("Memory warning");
			Lime.TexturePool.Instance.DiscardUnusedTextures(2);
			System.GC.Collect();
		}

		static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Write("========================= CRASH REPORT ============================\n" + e.ExceptionObject.ToString());
		}

		public override bool HandleOpenURL (UIApplication application, NSUrl url)
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
			Application.Instance.Active = true;
			AudioSystem.Active = true;
			Application.Instance.OnActivate();
		}
	
		public override void WillTerminate(UIApplication application)
		{
			if (WillTerminateEvent != null) {
				WillTerminateEvent();
			}
		}

		public override void OnResignActivation(UIApplication application)
		{
			// http://developer.apple.com/library/ios/#documentation/3DDrawing/Conceptual/OpenGLES_ProgrammingGuide/ImplementingaMultitasking-awareOpenGLESApplication/ImplementingaMultitasking-awareOpenGLESApplication.html#//apple_ref/doc/uid/TP40008793-CH5
			AudioSystem.Active = false;
			Application.Instance.OnDeactivate();
			GameController.Instance.View.RenderFrame();
			OpenTK.Graphics.ES11.GL.Finish();
			TexturePool.Instance.DiscardUnusedTextures(5);
			Application.Instance.Active = false;
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			if (RegisteredForRemoteNotificationsEvent != null) {
				RegisteredForRemoteNotificationsEvent(deviceToken);
			}
		}

		// This method is invoked when the application has loaded its UI and is ready to run
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			UIApplication.SharedApplication.StatusBarHidden = true;
			UIApplication.SharedApplication.IdleTimerDisabled = true;
		
			LaunchOptions = options;

			AudioSystem.Initialize();

			// create a new window instance based on the screen size
			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			GameController = new GameController();
			
			Window.RootViewController = GameController;

			// make the window visible
			Window.MakeKeyAndVisible();

			// Set the current directory.
			Directory.SetCurrentDirectory(NSBundle.MainBundle.ResourcePath);

			Application.Instance.OnCreate();

			GameView.Instance.Run();
			return true;
		}
	}
}
#endif