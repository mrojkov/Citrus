using AppKit;

namespace Tangerine
{
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
