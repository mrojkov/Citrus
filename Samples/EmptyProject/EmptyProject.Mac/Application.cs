using AppKit;
using System;

namespace EmptyProject.Mac
{
	static class Application
	{
		[STAThread]
		static void Main(string[] args)
		{
			if (Array.IndexOf(args, "--GenerateSerializationAssembly") >= 0) {
				Lime.Environment.GenerateSerializationAssembly("Serializer", typeof(EmptyProject.Application.AppData));
				return;
			}
			Lime.Application.Initialize(new Lime.ApplicationOptions { DecodeAudioInSeparateThread = true });
			NSApplication.SharedApplication.Delegate = new AppDelegate();
			Lime.Application.Run();
		}
	}
}