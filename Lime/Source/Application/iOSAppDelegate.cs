#if iOS
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Lime
{
	public class Application
	{
		internal static GameApp gameApp;
		
		public static void Main(string[] args, GameApp gameApp)
		{
			Application.gameApp = gameApp;
			UIApplication.Main(args, null, "AppDelegate");
		}
	}

	/// <summary>
	/// The UIApplicationDelegate for the application. This class is responsible for launching the 
	/// User Interface of the application, as well as listening(and optionally responding) to 
	/// application events from iOS.
	/// </summary>
	[Register("AppDelegate")]
	internal class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		GameController gameController;
		
		public override void ReceiveMemoryWarning(UIApplication application)
		{
			Lime.TexturePool.Instance.DiscardAllTextures();
		}

		public override void OnActivated(UIApplication application)
		{
			AudioSystem.Active = true;
			gameController.Activate();
			Application.gameApp.OnGLCreate();
		}

		public override void OnResignActivation(UIApplication application)
		{
			AudioSystem.Active = false;
			// Important: MonoTouch destroys OpenGL context on application hiding.
			// So, we must destroy all OpenGL objects.
			Lime.TexturePool.Instance.DiscardAllTextures();
			Application.gameApp.OnGLDestroy();
			gameController.Deactivate();
		}

		// This method is invoked when the application has loaded its UI and is ready to run
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			AudioSystem.Initialize();
			UIApplication.SharedApplication.StatusBarHidden = true;

			// create a new window instance based on the screen size
			window = new UIWindow(UIScreen.MainScreen.Bounds);

			gameController = new GameController();
			
			// in order to make iOS 3.0 compatible we use this:
			window.AddSubview(gameController.View);
			// instead of that:
			// window.RootViewController = gameController;

			// make the window visible
			window.MakeKeyAndVisible();

			// Set the current directory.
			Directory.SetCurrentDirectory(NSBundle.MainBundle.ResourcePath);

			Application.gameApp.OnCreate(gameController);

			return true;
		}
	}
}
#endif