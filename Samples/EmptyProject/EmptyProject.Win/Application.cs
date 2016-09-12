using System;
using EmptyProject.Application;

namespace EmptyProject.Win
{
	public class Application
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Lime.Application.Initialize(new Lime.ApplicationOptions());
			new EmptyProject.Application.Application();

			Lime.Application.Run();
		}
	}
}