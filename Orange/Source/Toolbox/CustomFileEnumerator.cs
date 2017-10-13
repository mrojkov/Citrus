using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orange
{
	public class CustomFilesEnumerator : IFileEnumerator
	{
		public string Directory { get; }
		public Predicate<FileInfo> EnumerationFilter { get; set; }
		private readonly List<FileInfo> files;

		public CustomFilesEnumerator(string directory, List<FileInfo> files)
		{
			Directory = directory;
			this.files = files;
		}

		public void Rescan() { }

		public List<FileInfo> Enumerate(string extension = null)
		{
			if (extension == null && EnumerationFilter == null) {
				return files;
			}
			return files
				.Where(file => extension == null || Path.GetExtension(file.Path) == extension)
				.Where(file => EnumerationFilter == null || EnumerationFilter(file))
				.ToList();
		}
	}
}
