namespace Orange
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Gtk.Application.Init();

			var window = new MainWindow();
			window.Show();

			Gtk.Application.Run();
		}
	}
}
