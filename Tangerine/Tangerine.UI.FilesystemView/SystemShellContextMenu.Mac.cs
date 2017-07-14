#if MAC
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Lime;

namespace Tangerine.UI.FilesystemView
{
	public class SystemShellContextMenu : ISystemShellContextMenu
	{
		public static ISystemShellContextMenu Instance { get; set; } = new SystemShellContextMenu();

		public void Show(string path)
		{
			Show(new[] { path });
		}

		public void Show(string[] multiplePaths)
		{
		}
	}
}
#endif