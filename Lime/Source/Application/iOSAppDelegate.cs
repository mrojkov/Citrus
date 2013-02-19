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

		// handlers
		public delegate bool OpenURLHandler(NSUrl url);
		public OpenURLHandler UrlOpened;

		// class-level declarations
		public UIWindow Window { get; private set; }
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
			return UIInterfaceOrientationMask.AllButUpsideDown;
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
			if (UrlOpened != null)
				return UrlOpened(url);
			else
				return base.HandleOpenURL(application, url);
		}
		
		public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			if (UrlOpened != null)
				return UrlOpened(url);
			else
				return base.OpenUrl(application, url, sourceApplication, annotation);
		}

		public override void OnActivated(UIApplication application)
		{
			AudioSystem.Active = true;
			GameController.Activate();
			Application.Instance.OnGLCreate();
		}

		public override void OnResignActivation(UIApplication application)
		{
			AudioSystem.Active = false;
			// Important: MonoTouch destroys OpenGL context on application hiding.
			// So, we must destroy all OpenGL objects.
			GameController.Deactivate();
			Lime.TexturePool.Instance.DiscardAllTextures();
			Application.Instance.OnGLDestroy();
		}

		// This method is invoked when the application has loaded its UI and is ready to run
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			UIApplication.SharedApplication.StatusBarHidden = true;
			UIApplication.SharedApplication.IdleTimerDisabled = true;
			
			AudioSystem.Initialize();

			// create a new window instance based on the screen size
			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			GameController = new GameController();
			
			string currSysVer = UIDevice.CurrentDevice.SystemVersion;
			if (currSysVer[0] >= '6') {
				Window.RootViewController = GameController;
				GameController.View.AutoResize = true;
			} else {
				Window.AddSubview(GameController.View);
			}

			// make the window visible
			Window.MakeKeyAndVisible();

			// Set the current directory.
			Directory.SetCurrentDirectory(NSBundle.MainBundle.ResourcePath);

			Application.Instance.OnCreate();
			return true;
		}
	}
}
#endif