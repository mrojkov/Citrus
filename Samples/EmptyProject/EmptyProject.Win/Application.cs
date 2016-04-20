using System;
using EmptyProject.Application;

namespace EmptyProject.Win
{
	public class Application
	{
		private static Type[] GetSerializationTypes()
		{
			var result = new [] {
				typeof(AppData)
			};
			return result;
		}

		[STAThread]
		public static void Main(string[] args)
		{
			if (Array.IndexOf(args, "--GenerateSerializationAssembly") >= 0)
			{
				Lime.Environment.GenerateSerializationAssembly("Serializer", GetSerializationTypes());
				return;
			}
			Lime.Application.Initialize(new Lime.ApplicationOptions());
			new EmptyProject.Application.Application();
			Lime.Application.Run();
		}
	}
}