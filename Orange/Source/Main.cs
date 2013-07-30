using System;

namespace Orange
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
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
