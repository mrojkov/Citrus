using System;
using AppKit;

namespace EmptyProject.Mac
{
	static class MainClass
	{
		static void Main(string[] arg)
		{
			NSApplication.Init();
			var app = new EmptyProject.Application(arg, new Lime.Application.StartupOptions());
			using (var gameView = new Lime.GameView(app)) {
				gameView.Run(60);
			}
		}
	}
}

