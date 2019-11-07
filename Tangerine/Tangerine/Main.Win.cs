#if WIN
using System;
using System.Collections.Generic;
using Lime;
using Lime.KGDCitronLifeCycle;

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
			// TODO Remove it after KGD will be migrated
			CitronLifeCycle.Initialize();
			TangerineApp.Initialize(args);
			Lime.Application.Run();
		}
	}
}
#endif
