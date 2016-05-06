using System;

using Foundation;
using AppKit;
using Lime;

namespace EmptyProject.Mac 
{
	public class AppDelegate : NSApplicationDelegate 
	{
		public override void DidFinishLaunching(NSNotification notification) 
		{
			var appName = NSProcessInfo.ProcessInfo.ProcessName;
			CreateMenu(appName);
			new EmptyProject.Application.Application();
		}

		void CreateMenu(string appName) 
		{
			var mainMenu = new NSMenu();
			var appMenu = new NSMenu();
			var appMenuItem = new NSMenuItem {
				Submenu = appMenu
			};
			mainMenu.AddItem(appMenuItem);
			var quitMenuItem = new NSMenuItem(string.Format("Quit {0}", appName), "q", delegate {
				NSApplication.SharedApplication.Terminate(mainMenu);
			});
			appMenu.AddItem(quitMenuItem);
			NSApplication.SharedApplication.MainMenu = mainMenu;
		}
	}
}

