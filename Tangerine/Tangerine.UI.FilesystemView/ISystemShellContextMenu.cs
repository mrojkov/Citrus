using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.FilesystemView
{
	public interface ISystemShellContextMenu
	{
		void Show(string path);
		void Show(IEnumerable<string> paths);
		void Show(string path, Vector2 position);
		void Show(IEnumerable<string> paths, Vector2 position);
	}
}