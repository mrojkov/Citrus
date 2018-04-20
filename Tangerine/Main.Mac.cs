#if MAC
using AppKit;
using Foundation;
using Tangerine.Core;

namespace Tangerine
{
	static class MainClass
	{
		static void Main(string[] args)
		{
			Lime.Application.Initialize();
            NSApplication.SharedApplication.DidFinishLaunching += (sender, e) => TangerineApp.Initialize(args);
			Lime.Application.Run();
		}
	}
}
#endif