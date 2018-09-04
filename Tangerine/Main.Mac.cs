#if MAC
using System.Linq;
using AppKit;
using Foundation;
using Tangerine.Core;

namespace Tangerine
{
	static class MainClass
	{
		static void Main(string[] args)
		{
			args = args.Where(i => !i.StartsWith("-psn")).ToArray(); // Skip Xamarin debugger specific arguments.
			Lime.Application.Initialize();
            NSApplication.SharedApplication.DidFinishLaunching += (sender, e) => TangerineApp.Initialize(args);
			Lime.Application.Run();
		}
	}
}
#endif