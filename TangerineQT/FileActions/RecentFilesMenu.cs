using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qyoto;

namespace Tangerine
{
	public class RecentFilesMenu : Menu
	{
		public static RecentFilesMenu Instance = new RecentFilesMenu();

		RecentFilesMenu() 
			: base("Recent Files")
		{
		}
	}
}
