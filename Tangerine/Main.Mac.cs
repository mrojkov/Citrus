#if MAC
using AppKit;
using Foundation;
using Tangerine.Core;

namespace Tangerine
{
	public class AppDelegate : NSApplicationDelegate
	{
		public override void DidFinishLaunching(NSNotification notification)
		{
			TangerineApp.Initialize();
		}
	}
	
	static class MainClass
	{
		static void Main(string[] args)
		{
			Lime.Application.Initialize();
			NSApplication.SharedApplication.Delegate = new AppDelegate();
			Lime.Application.Run();
		}
	}
}
#endif