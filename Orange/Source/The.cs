using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	public static class The
	{
		public static Workspace Workspace { get { return Workspace.Instance; } }
		public static MainWindow MainWindow { get { return MainWindow.Instance; } }
		public static MenuController MenuController { get { return MenuController.Instance; } }
	}
}
