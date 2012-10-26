#if iOS
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Runtime.InteropServices;

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

		// class-level declarations
		public UIWindow Window { get; private set; }
		GameController gameController;

		public AppDelegate()
		{
			Instance = this;
			AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;
		}
		
		public override void ReceiveMemoryWarning(UIApplication application)
		{
			Logger.Write("Memory warning");
			Lime.TexturePool.Instance.DiscardUnusedTextures(2);
			System.GC.Collect();
		}

		static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Write(e.ExceptionObject.ToString());
		}

		public override void OnActivated(UIApplication application)
		{
			AudioSystem.Active = true;
			gameController.Activate();
			Application.Instance.OnGLCreate();
		}

		public override void OnResignActivation(UIApplication application)
		{
			AudioSystem.Active = false;
			// Important: MonoTouch destroys OpenGL context on application hiding.
			// So, we must destroy all OpenGL objects.
			gameController.Deactivate();
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

			gameController = new GameController();
			
			string currSysVer = UIDevice.CurrentDevice.SystemVersion;
			if (currSysVer[0] >= '6') {
				Window.RootViewController = gameController;
			} else {
				Window.AddSubview(gameController.View);
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