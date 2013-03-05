namespace Orange
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// TODO: implement somehow in a different way:
			//var frameModel = Lime.Serialization.Serializer[typeof(Lime.Frame)];
			//frameModel.AddSubType(101, typeof(Kumquat.Clickable));

			Gtk.Application.Init();
			MainWindow window = new MainWindow();
			window.Title = "Citrus Aurantium"; 	// Ха-ха, Ксюха купила аромомасло "сладкий апельсин"
			window.Show();
			Gtk.Application.Run();
		}
	}
}
