#if MAC
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		public void Show(IEnumerable<string> paths)
		{
		}

		public void Show(string path, Vector2 position)
		{
		}

		public void Show(IEnumerable<string> paths, Vector2 position)
		{
		}
	}
}
#endif