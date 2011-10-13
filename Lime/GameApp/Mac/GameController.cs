#if MAC
using System;
using MonoMac.AppKit;
using MonoMac.OpenGL;
using System.Drawing;
using System.IO;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace Lime
{
	internal class GameController : NSObject, IGameWindow
	{
		NSWindow window;		
		GameView view;
		Size windowSize;
		
		bool isInFullScreenMode;
	
		// Put the main menu stuff here to prevent a garbage collection
		NSMenu mainMenu;
		NSMenu appMenu;
		NSMenuItem appMenuItem;
		NSMenuItem separatorMenuItem;
		NSMenuItem fullScreenMenuItem;
		NSMenuItem quitMenuItem;
		
		public GameController (IGameApp game)
		{		
			// The default resolution is 640 x 480
			windowSize = new Size (640, 480);
			RectangleF frame = new RectangleF (0, 0, windowSize.Width, windowSize.Height);
			
			// Create a window
			window = new NSWindow (frame, NSWindowStyle.Titled | NSWindowStyle.Closable, NSBackingStore.Buffered, true);
			window.Center ();
			window.IsOpaque = true;
			
			view = new GameView (frame, null, game);
			
			window.ContentView.AddSubview (view);
			window.AcceptsMouseMovedEvents = false;
			
			// Attach MacOS application main menu
			SetMainMenu ();
						
			// Set the current directory.
			// We set the current directory to the ResourcePath on Mac
			Directory.SetCurrentDirectory (NSBundle.MainBundle.ResourcePath);
			
			game.OnCreate (this);
			window.MakeKeyAndOrderFront (window);			
		}
		
		void OnFullScreen (Object sender, EventArgs e)
		{
			FullScreen = !FullScreen;
		}

		void OnQuit (Object sender, EventArgs e)
		{
			NSApplication.SharedApplication.Terminate (new NSObject());
		}

		void SetMainMenu ()
		{
			mainMenu = new NSMenu ("MainMenu");
			appMenu = new NSMenu ("Application");
			appMenuItem = mainMenu.AddItem ("Application", null, "");
			mainMenu.SetSubmenu (appMenu, appMenuItem);
			fullScreenMenuItem = new NSMenuItem ("Toggle FullScreen", "f", OnFullScreen);
			appMenu.AddItem (fullScreenMenuItem);
			separatorMenuItem = new NSMenuItem ("");
			appMenu.AddItem (separatorMenuItem);
			quitMenuItem = new NSMenuItem ("Quit", "q", OnQuit);
			appMenu.AddItem (quitMenuItem);
			NSApplication.SharedApplication.SetMainMenu (mainMenu);	
		}
		
		public DeviceOrientation CurrentDeviceOrientation {
			get { return DeviceOrientation.LandscapeLeft; }
		}
		
		public Size WindowSize {
			// returs actual window size
			get {
				return (Size)view.Size;
			}
			// sets window size for windowed mode
			set {
				windowSize = value;
				if (!isInFullScreenMode) {
					ResetWindowBounds ();
				}				
			}			
		}	
				
		public void Activate ()
		{
			view.Run ();
		}
		
		public void Deactivate ()
		{
			view.Stop ();
		}
		
		public bool FullScreen {
			get { return isInFullScreenMode; }
			set {
				if (isInFullScreenMode != value) {
					isInFullScreenMode = value;
					if (isInFullScreenMode)
						GoFullScreenMode ();
					else
						GoWindowMode ();		
				}
			}
		}

		private float TitleBarHeight ()
		{
			RectangleF contentRect = NSWindow.ContentRectFor (window.Frame, window.StyleMask);
			return window.Frame.Height - contentRect.Height;
		}

		private void ResetWindowBounds ()
		{
			RectangleF frame;
			RectangleF content;
			
			if (isInFullScreenMode) {
				frame = NSScreen.MainScreen.Frame;
				content = NSScreen.MainScreen.Frame;
				window.SetFrame (frame, true);
			} else {
				content = view.Bounds;
				content.Width = Math.Min (windowSize.Width, NSScreen.MainScreen.VisibleFrame.Width);
				content.Height = Math.Min (windowSize.Height, NSScreen.MainScreen.VisibleFrame.Height - TitleBarHeight ());
				
				frame = window.Frame;
				frame.X = Math.Max (frame.X, NSScreen.MainScreen.VisibleFrame.X);
				frame.Y = Math.Max (frame.Y, NSScreen.MainScreen.VisibleFrame.Y);
				frame.Width = content.Width;
				frame.Height = content.Height + TitleBarHeight();
	
				window.SetFrame (frame, true);
				window.Center ();
			}
			
			view.Bounds = content;
			view.Size = content.Size.ToSize();	
		}
		
		private void GoFullScreenMode ()
		{
			isInFullScreenMode = true;
			
			// Some games set fullscreen in their initialize function,
			// before we have sized the window and set it active.
			// Do that now, or else mouse tracking breaks.
			window.MakeKeyAndOrderFront (window);
			ResetWindowBounds();
			
			// Changing window style resets the title. Save it.
			string oldTitle = view.Title;
			
			NSMenu.MenuBarVisible = false;
			window.StyleMask = NSWindowStyle.Borderless;
			window.HidesOnDeactivate = true;
			
			ResetWindowBounds();
			
			if (oldTitle != null)
				view.Title = oldTitle;
		}

		private void GoWindowMode ()
		{
			isInFullScreenMode = false;
				
			// Changing window style resets the title. Save it.
			string oldTitle = view.Title;
			
			NSMenu.MenuBarVisible = true;
			window.StyleMask = NSWindowStyle.Titled | NSWindowStyle.Closable;
			window.HidesOnDeactivate = false;
			
			ResetWindowBounds();
			
			if (oldTitle != null)
				view.Title = oldTitle;		
		}

		public float FrameRate {
			get {
				return view.FrameRate;
			}
		}

	}
}
#endif