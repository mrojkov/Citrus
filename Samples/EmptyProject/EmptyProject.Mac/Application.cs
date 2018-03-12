using AppKit;
using System;
using Foundation;

namespace EmptyProject.Mac
{
	static class Application
	{
		[STAThread]
		static void Main(string[] args)
		{
			Lime.Application.Initialize(new Lime.ApplicationOptions { DecodeAudioInSeparateThread = true });
			NSApplication.SharedApplication.DidFinishLaunching += SharedApplication_DidFinishLaunching;;
			Lime.Application.Run();
		}

		static void SharedApplication_DidFinishLaunching (object sender, EventArgs e)
		{
			var appName = NSProcessInfo.ProcessInfo.ProcessName;
			CreateMenu(appName);
			EmptyProject.Application.Application.Initialize();
		}

		static void CreateMenu(string appName) 
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