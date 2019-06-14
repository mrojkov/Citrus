#if WIN
using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine
{
	public class MainApplication
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var thisExe = System.Reflection.Assembly.GetExecutingAssembly();
			string [] resources = thisExe.GetManifestResourceNames();
			Lime.Application.Initialize(new ApplicationOptions {
				RenderingBackend = RenderingBackend.Vulkan
			});
			TangerineApp.Initialize(args);
			Lime.Application.Run();
		}
	}
}
#endif
