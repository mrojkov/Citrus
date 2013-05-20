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
			Workspace.Open("f:/Work/Zx3/Kill3.citproj");
			//Workspace.Open("/Users/buzer2010/Desktop/dev/net/Zzz/Main/Kill3.citproj");
			The.Workspace.OpenDocument("f:/Work/Zx3/Data/Game/Alien.tan");
			//The.Workspace.OpenDocument("/Users/buzer2010/Desktop/dev/net/Zzz/Main/Data/Game/Alien.tan");
			//The.Workspace.OpenDocument("f:/Work/Zzz/Data/Game/Levels/C_Bridge/Bridge02.tan");
			var mainWindow = new MainWindow();
			ShowTimeline();
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
