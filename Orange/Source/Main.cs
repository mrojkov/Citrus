namespace Orange
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var frameModel = Lime.Serialization.Serializer[typeof(Lime.Frame)];
			frameModel.AddSubType(101, typeof(Kumquat.Clickable));

			Gtk.Application.Init();
			MainWindow window = new MainWindow();
			window.Show();
			Gtk.Application.Run();
		}
	}
}
