using System;
using System.Collections.Generic;
using System.Linq;
using Xwt;
using Xwt.Drawing;

namespace Tangerine
{
	public class App
	{
		[STAThread]
		static void Main()
		{
			//Application.Initialize("XwtPlus.WPFBackend.WPFEngine, XwtPlus.WPF, Version=1.00");
			Application.Initialize("XwtPlus.GtkBackend.GtkEngine, XwtPlus.Gtk, Version=1.00");

			//Application.Initialize(ToolkitType.Wpf);
			//Workspace.Open("f:/Work/Zzz/Kill3.citproj");
			//The.Workspace.OpenDocument("f:/Work/Zzz/Data/Game/Alien.tan");
			//The.Workspace.OpenDocument("f:/Work/Zzz/Data/Game/Levels/C_Bridge/Bridge02.tan");
			//ShowTimeline();
			var mainWindow = new MainWindow();
			//mainWindow.Show();
			XwtPlus.CommandManager.Realize();
			Application.Run();
			//mainWindow.Dispose();
		}

		private static void ShowTimeline()
		{
			The.Document.RebuildRows();
			The.Timeline.Refresh();
		}
	}
}
