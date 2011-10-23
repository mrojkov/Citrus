using System;
using System.IO;
using Lime;
using Lemon;
#if MAC
using MonoMac.Foundation;
#endif
using System.Diagnostics;
using System.Runtime.InteropServices;

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
