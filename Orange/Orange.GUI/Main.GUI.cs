using System;
using Lime;

namespace Orange
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			PluginLoader.RegisterAssembly(typeof(MainClass).Assembly);
			var thisExe = System.Reflection.Assembly.GetExecutingAssembly();
			var resources = thisExe.GetManifestResourceNames();
			Application.Initialize(new ApplicationOptions {
				RenderingBackend = RenderingBackend.OpenGL
			});
			OrangeApp.Initialize();
			Application.Run();
		}
	}
}
