using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orange
{
	public struct FileInfo
	{
		public string Path;
		public DateTime LastWriteTime;
	}

	public class FileEnumerator
	{
		public string Directory { get; private set; }

		public Predicate<FileInfo> EnumerationFilter;

		List<FileInfo> files = new List<FileInfo>();

		public FileEnumerator(string directory)
		{
			Directory = directory;
			Rescan();
		}

		public void Rescan()
		{
			files.Clear();
			var dirInfo = new System.IO.DirectoryInfo(Directory);

			foreach (var fileInfo in dirInfo.GetFiles("*.*", SearchOption.AllDirectories)) {
				var file = fileInfo.FullName;
				if (file.Contains(".svn"))
					continue;
				file = file.Remove(0, dirInfo.FullName.Length + 1);
				file = Lime.AssetPath.CorrectSlashes(file);
				files.Add(new FileInfo { Path = file, LastWriteTime = fileInfo.LastWriteTime });
			}
		}

		public List<FileInfo> Enumerate(string extension = null)
		{
			if (extension == null && EnumerationFilter == null) {
				return files;
			}
			return files.Where(file => extension == null || Path.GetExtension(file.Path) == extension)
				.Where(file => EnumerationFilter == null || EnumerationFilter(file))
				.ToList();
		}
	}
}
