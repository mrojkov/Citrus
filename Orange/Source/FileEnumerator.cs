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
				files.Add(new FileInfo { Path = file, LastWriteTime = fileInfo.LastWriteTime });
			}
		}

		public List<FileInfo> Enumerate(string extension = null)
		{
			if (extension == null) {
				return files;
			}
			List<FileInfo> result = new List<FileInfo>();
			foreach (FileInfo file in files) {
				if (Path.GetExtension(file.Path) != extension)
					continue;
				result.Add(file);
			}
			return result;
		}
	}
}
