using System;

namespace Orange
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			if (Array.IndexOf(args, "-console") >= 0) {
				ConsoleMode();
			} else {
				GuiMode();
			}
		}

		static void ConsoleMode()
		{
			Console.WriteLine("Hello from Orange!");
		}

		static void GuiMode()
		{
			Gtk.Application.Init();
			new MainWindow();
			CreateMenuItems();
			The.Workspace.Load();
			Gtk.Application.Run();
		}

		private static void CreateMenuItems()
		{
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			The.MenuController.CreateAssemblyMenuItems(assembly);
		}
	}
}
