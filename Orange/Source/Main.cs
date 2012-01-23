namespace Orange
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Gtk.Application.Init();
			MainWindow window = new MainWindow();
			window.Show();
			Gtk.Application.Run();
		}
	}
}
