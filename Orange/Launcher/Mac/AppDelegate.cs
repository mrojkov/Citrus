using AppKit;
using Foundation;

namespace Launcher
{
	internal partial class AppDelegate : NSApplicationDelegate
	{
		public static CommonBuilder Builder;
		public static CommandLineArguments Args;
		MainWindowController mainWindowController;

		public AppDelegate()
		{
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
			mainWindowController = new MainWindowController();
			Builder.OnBuildSuccess += () => InvokeOnMainThread(() => NSApplication.SharedApplication.Terminate(this));
			Builder.OnBuildStatusChange += mainWindowController.SetBuildStatus;
			Builder.OnBuildFail += mainWindowController.ShowLog;
			System.Console.SetOut (mainWindowController.LogWriter);
			System.Console.SetError (mainWindowController.LogWriter);
			mainWindowController.Window.MakeKeyAndOrderFront(this);
			Builder.Start(!Args.JustBuild);
		}

		public override void WillTerminate(NSNotification notification)	
		{
			// Insert code here to tear down your application
		}
	}
}
