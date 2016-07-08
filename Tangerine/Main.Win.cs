#if WIN
using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine
{
	public class MainApplication
	{
		private static List<Type> GetSerializationTypes()
		{
			var result = new List<Type>() {
				//typeof(Type)
			};
			return result;
		}

		[STAThread]
		public static void Main(string[] args)
		{
			if (Array.IndexOf(args, "--GenerateSerializationAssembly") >= 0) {
				Lime.Environment.GenerateSerializationAssembly("Serializer", GetSerializationTypes().ToArray());
				return;
			}

			var thisExe = System.Reflection.Assembly.GetExecutingAssembly();
			string [] resources = thisExe.GetManifestResourceNames();
			Lime.Application.Initialize(new ApplicationOptions {
				RenderingBackend = RenderingBackend.OpenGL
			});
			TangerineApp.Initialize();
			Lime.Application.Run();
		}
	}
}
#endif