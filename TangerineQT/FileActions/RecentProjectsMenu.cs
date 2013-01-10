using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public class RecentProjectsMenu : Menu
	{
		public static RecentProjectsMenu Instance = new RecentProjectsMenu();

		RecentProjectsMenu()
			: base("Recent Projects")
		{
		}
	}
}
