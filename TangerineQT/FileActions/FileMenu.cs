using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qyoto;

namespace Tangerine
{
	public class FileMenu : Menu
	{
		public static FileMenu Instance = new FileMenu();

		FileMenu()
			: base("&File")
		{
			Add(OpenFileAction.Instance);
			Add(OpenProjectAction.Instance);
			AddSeparator();
			Add(RecentProjectsMenu.Instance);
			AddSeparator();
			Add(ExitAction.Instance);
		}
	}
}
