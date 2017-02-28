using System;

namespace EmptyProject.Win
{
	public class Application
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Lime.Application.Initialize(new Lime.ApplicationOptions());
			EmptyProject.Application.Application.Initialize();
			Lime.Application.Run();
		}
	}
}