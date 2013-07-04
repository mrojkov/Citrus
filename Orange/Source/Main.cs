namespace Orange
{
	class MainClass
	{
		public static void Main (string[] args)
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
