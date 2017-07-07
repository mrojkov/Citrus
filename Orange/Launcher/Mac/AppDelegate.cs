using System;
using AppKit;
using Foundation;

namespace Launcher
{
	internal partial class AppDelegate : NSApplicationDelegate
	{
		public static Action<AppDelegate> OnFinishLaunching;
		public MainWindowController MainWindowController;

		public override void DidFinishLaunching(NSNotification notification)
		{
			MainWindowController = new MainWindowController();
			Console.SetOut(MainWindowController.LogWriter);
			Console.SetError(MainWindowController.LogWriter);
			MainWindowController.Window.MakeKeyAndOrderFront(this);
			OnFinishLaunching(this);
		}

		public override void WillTerminate(NSNotification notification)	
		{
			// Insert code here to tear down your application
		}
	}
}
