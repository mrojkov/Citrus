using System;

#if MAC
using MonoMac.AppKit;
#endif

namespace Orange
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
#if MAC
			NSApplication.Init();
#endif

			if (Toolbox.GetCommandLineFlag("--console") || Toolbox.GetCommandLineFlag("--help")) {
				ConsoleMode(args);
			} else {
				GuiMode();
			}
		}

		static void ConsoleMode(string[] args)
		{
			UserInterface.CreateConsole();
		}

		static void GuiMode()
		{
			UserInterface.CreateGUI();
		}
	}
}
