#if MAC
using System;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Lime
{
	public class AppDelegate : NSApplicationDelegate
	{
		GameApp game;
		GameController gameController;
		
		public AppDelegate (GameApp game)
		{
			this.game = game;
		}
		
		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			gameController = new GameController (game);
		}
		
		public override void DidBecomeActive (NSNotification notification)
		{
			if (gameController != null) {
				gameController.Activate ();
			}
		}
		
		public override void DidResignActive (NSNotification notification)
		{
			if (gameController != null) {
				gameController.Deactivate ();
			}
		}
		
		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}
	}
}
#endif