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
	// for each folder in target directory also enumerates folder/#CookingRules.txt if present
	// so it can be applied to this folder
	public class FileEnumerator : IFileEnumerator
	{
		public string Directory { get; }
		public Predicate<Orange.FileInfo> EnumerationFilter { get; set; }
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

		private void TryAddCookingRulesInDirectory(string FullName, ref string cookingRulesPath)
		{
			cookingRulesPath = Path.Combine(FullName, CookingRulesBuilder.CookingRulesFilename);
			if (File.Exists(cookingRulesPath)) {
				var fi = new System.IO.FileInfo(cookingRulesPath);
				files.Add(new Orange.FileInfo { Path = ProcessPath(cookingRulesPath), LastWriteTime = fi.LastWriteTime });
			}
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
				TryAddCookingRulesInDirectory(di.FullName, ref cookingRulesPath);
			}
			string innerCookingRulesPath = null;
			foreach (var fileInfo in dirInfoTarget.GetFileSystemInfos("*.*", SearchOption.TopDirectoryOnly)) {
				var file = fileInfo.FullName;
				if (file.Contains(".svn"))
					continue;
				if (file == cookingRulesPath) {
					continue;
				}
				files.Add(new Orange.FileInfo { Path = ProcessPath(file), LastWriteTime = fileInfo.LastWriteTime });
				if (fileInfo.Attributes == FileAttributes.Directory) {
					TryAddCookingRulesInDirectory(fileInfo.FullName, ref innerCookingRulesPath);
				}
			}
		}

		public IEnumerable<Orange.FileInfo> Enumerate(string extension = null)
		{
			if (extension == null && EnumerationFilter == null) {
				return files;
			}
			return files.Where(file => extension == null || file.Path.EndsWith(extension))
				.Where(file => EnumerationFilter == null || EnumerationFilter(file));
		}
	}
}