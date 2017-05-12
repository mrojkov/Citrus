using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orange;

namespace Tangerine.UI.FilesystemView
{
	// Enumerates all parent directories from target directory to root directory
	// also enumerating all #CookingRules.txt files by the way
	// afterwards enumerates top level files and folders in target directory
	public class FileEnumerator : IFileEnumerator
	{
		public string Directory { get; }
		public Predicate<Orange.FileInfo> EnumerationFilter;
		readonly List<Orange.FileInfo> files = new List<Orange.FileInfo>();
		private readonly string targetDirectory;
		DirectoryInfo dirInfoRoot;
		DirectoryInfo dirInfoTarget;

		public FileEnumerator(string rootDirectory, string targetDirectory)
		{
			Directory = rootDirectory;
			this.targetDirectory = targetDirectory;
			dirInfoRoot = new DirectoryInfo(Directory);
			dirInfoTarget = new DirectoryInfo(targetDirectory);
			Rescan();
		}

		private string ProcessPath(string path)
		{
			string r = path;
			r = r.Remove(0, dirInfoRoot.FullName.Length);
			r = CsprojSynchronization.ToUnixSlashes(r);
			if (r.StartsWith("/")) {
				r = r.Substring(1);
			}
			return r;
		}

		public void Rescan()
		{
			files.Clear();
			List<DirectoryInfo> trunc = new List<DirectoryInfo>();

			var dirInfo = dirInfoTarget;
			trunc.Add(dirInfo);
			while (dirInfo.FullName != dirInfoRoot.FullName) {
				dirInfo = dirInfo.Parent;
				trunc.Add(dirInfo);
			}
			trunc.Reverse();
			string cookingRulesPath = "";
			foreach (var di in trunc) {
				files.Add(new Orange.FileInfo { Path = ProcessPath(di.FullName), LastWriteTime = di.LastWriteTime });
				cookingRulesPath = Path.Combine(di.FullName, "#CookingRules.txt");
				if (File.Exists(cookingRulesPath)) {
					var fi = new System.IO.FileInfo(cookingRulesPath);
					files.Add(new Orange.FileInfo { Path = ProcessPath(cookingRulesPath), LastWriteTime = fi.LastWriteTime });
				}
			}

			foreach (var fileInfo in dirInfoTarget.GetFileSystemInfos("*.*", SearchOption.TopDirectoryOnly)) {
				var file = fileInfo.FullName;
				if (file.Contains(".svn"))
					continue;
				if (file == cookingRulesPath) {
					continue;
				}
				files.Add(new Orange.FileInfo { Path = ProcessPath(file), LastWriteTime = fileInfo.LastWriteTime });
			}
		}

		public List<Orange.FileInfo> Enumerate(string extension = null)
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