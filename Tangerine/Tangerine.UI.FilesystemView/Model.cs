using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI.FilesystemView
{
	public class Model
	{
		public HashSet<string> Selection = new HashSet<string>();
		private string currentPath = Project.Current?.AssetsDirectory ?? Directory.GetCurrentDirectory();
		public string CurrentPath
		{
			get { return currentPath; }
			set
			{
				try {
					Directory.EnumerateFileSystemEntries(value).ToList();
					currentPath = value;
				} catch {
					(new AlertDialog("Invalid Directory")).Show();
				}
			}
		}

		public void GoUp()
		{
			var p = Directory.GetParent(CurrentPath);
			if (p == null) {
				return;
			}
			CurrentPath = p.FullName;
		}

		public IEnumerable<string> EnumerateItems()
		{
			return Directory.EnumerateFileSystemEntries(CurrentPath);
		}

		public void GoTo(string path)
		{
			FileAttributes attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
				CurrentPath = path;
			}
		}
	}
}