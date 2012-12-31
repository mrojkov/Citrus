using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class TangerineApp
	{
		[STAThread]
		public static int Main(String[] args)
		{
			var app = new QApplication(args);
			CreateMainWindow();
			ActionManager.Instance.Initialize();
			return QApplication.Exec();
		}

		private static void CreateMainWindow()
		{
			MainWindow.Instance.Initialize();
		}
	}
}
