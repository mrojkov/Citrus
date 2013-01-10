using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Qyoto;

namespace Tangerine
{
	public class Tangerine
	{
		[STAThread]
		public static int Main(String[] args)
		{
			System.Diagnostics.Debug.AutoFlush = true;
			var app = new QApplication(args);
			CreateMainWindow();			
			RecentProjectsManager.Instance.Initialize();
			OpenLastProject();
			return QApplication.Exec();
		}

		public static string DataDirectory = Lime.Environment.GetDataDirectory("Tangerine");

		public static string GetDataFilePath(string name)
		{
			return Path.Combine(DataDirectory, name);
		}

		private static void CreateMainWindow()
		{
			MainWindow.Instance.Initialize();
			MainWindow.Instance.AddMenu(FileMenu.Instance);
			MainWindow.Instance.AddMenu(ViewMenu.Instance);
		}

		private static void OpenLastProject()
		{
			var file = RecentProjectsManager.Instance.GetFirstItem();
			if (file != null) {
				OpenProjectAction.Instance.OpenProject(file);
			}
		}
	}
}
