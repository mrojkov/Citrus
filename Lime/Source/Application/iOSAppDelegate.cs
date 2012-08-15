#if iOS
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

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
		// class-level declarations
		public UIWindow Window { get; private set; }
		GameController gameController;

		public AppDelegate()
		{
			AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;
		}
		
		public override void ReceiveMemoryWarning(UIApplication application)
		{
			Lime.TexturePool.Instance.DiscardUnusedTextures(2);
			System.GC.Collect();
		}

		static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			//string appId = Path.GetFileName(NSBundle.MainBundle.ExecutablePath);
			//string currentDateTime = DateTime.Now.ToString();
			//string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			//string fileName = string.Format("{0} {1} Crash.txt", appId, currentDateTime);
			//string filePath = Path.Combine(path, fileName);
			Console.WriteLine(e.ToString());
			//File.WriteAllText(filePath, e.ToString());
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
			Lime.TexturePool.Instance.DiscardAllTextures();
			Application.Instance.OnGLDestroy();
			gameController.Deactivate();
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
			
			// in order to make iOS 3.0 compatible we use this:
			Window.AddSubview(gameController.View);
			// instead of that:
			// window.RootViewController = gameController;

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