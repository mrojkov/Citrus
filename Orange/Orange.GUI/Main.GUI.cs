#if WIN
using System;
#elif MAC
using AppKit;
#endif
using Lime;

namespace Orange
{
	class MainClass
	{
#if WIN
		[STAThread]
		public static void Main(string[] args)
		{
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			PluginLoader.RegisterAssembly(typeof(MainClass).Assembly);
			var thisExe = System.Reflection.Assembly.GetExecutingAssembly();
			var resources = thisExe.GetManifestResourceNames();
			Application.Initialize(new ApplicationOptions {
				RenderingBackend = RenderingBackend.Vulkan
			});
			OrangeApp.Initialize();
			Application.Run();
		}
#elif MAC
		static void Main(string[] args)
		{
			Application.Initialize();
			NSApplication.SharedApplication.DidFinishLaunching += (sender, e) => OrangeApp.Initialize();
			Application.Run();
		}
#endif
	}
}
