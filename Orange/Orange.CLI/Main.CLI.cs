using System;

namespace Orange
{
	class MainClass
	{
#if WIN
		[STAThread]
#endif
		static void Main(string[] args)
		{
#if MAC
			Lime.Application.Initialize();
#endif
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			PluginLoader.RegisterAssembly(typeof(MainClass).Assembly);
			UserInterface.Instance = new ConsoleUI();
			UserInterface.Instance.Initialize();
		}
	}
}
