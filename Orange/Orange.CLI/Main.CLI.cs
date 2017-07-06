using System;

namespace Orange
{
	class MainClass
	{
		static void Main(string[] args)
		{
			Lime.Application.Initialize();
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			PluginLoader.RegisterAssembly(typeof(MainClass).Assembly);
			UserInterface.Instance = new ConsoleUI();
			UserInterface.Instance.Initialize();
		}
	}
}
