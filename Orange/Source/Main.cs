using System;

namespace Orange
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;

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
