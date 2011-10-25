namespace Orange
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Gtk.Application.Init ();
			MainDialog dlg = new MainDialog ();
			dlg.Run ();
		}
	}
}
