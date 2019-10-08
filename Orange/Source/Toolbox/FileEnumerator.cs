using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orange
{
	public struct FileInfo
	{
		public string Path;
		public DateTime LastWriteTime;
	}

	public class FileEnumerator : IFileEnumerator
	{
		public string Directory { get; }
		public Predicate<FileInfo> EnumerationFilter { get; set; }
		readonly List<FileInfo> files = new List<FileInfo>();

		public FileEnumerator(string directory)
		{
			Directory = directory;
			Rescan();
		}

		public void Rescan()
		{
			files.Clear();
			var dirInfo = new DirectoryInfo(Directory);

			foreach (var fileInfo in dirInfo.GetFiles("*.*", SearchOption.AllDirectories)) {
				var file = fileInfo.FullName;
				if (file.Contains(".svn"))
					continue;
				file = file.Remove(0, dirInfo.FullName.Length + 1);
				file = CsprojSynchronization.ToUnixSlashes(file);
				files.Add(new FileInfo { Path = file, LastWriteTime = fileInfo.LastWriteTime });
			}
#if MAC
			// Mono 6.0 breaks files order
			files.Sort((a, b) => string.Compare(a.Path, b.Path));
#endif
		}

		public IEnumerable<FileInfo> Enumerate(string extension = null)
		{
			if (extension == null && EnumerationFilter == null) {
				return files;
			}
			return files
				.Where(file => extension == null || file.Path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
				.Where(file => EnumerationFilter == null || EnumerationFilter(file));
		}
	}

	/// <summary>
	/// File enumerator optimized for scaning large data sets with some unwanted sub-directories
	/// </summary>
	public class ScanOptimizedFileEnumerator : IFileEnumerator
	{
		private readonly Predicate<DirectoryInfo> scanFilter;
		private readonly List<FileInfo> files = new List<FileInfo>();
		private readonly bool cutDirectoryPrefix = true;

		public ScanOptimizedFileEnumerator(string directory, Predicate<DirectoryInfo> scanFilter, bool cutDirectoryPrefix = true)
		{
			this.scanFilter = scanFilter;
			this.cutDirectoryPrefix = cutDirectoryPrefix;
			Directory = directory;
			Rescan();
		}

		public string Directory { get; }
		public Predicate<FileInfo> EnumerationFilter { get; set; }

		public void Rescan()
		{
			files.Clear();
			var dirInfo = new DirectoryInfo(Directory);
			var queue = new Queue<DirectoryInfo>();
			queue.Enqueue(new DirectoryInfo(Directory));
			while (queue.Count != 0) {
				var rootDirectoryInfo = queue.Dequeue();
				foreach (var fileInfo in rootDirectoryInfo.EnumerateFiles()) {
					var file = fileInfo.FullName;
					if (cutDirectoryPrefix) {
						file = file.Remove(0, dirInfo.FullName.Length + 1);
					}
					file = CsprojSynchronization.ToUnixSlashes(file);
					files.Add(new FileInfo { Path = file, LastWriteTime = fileInfo.LastWriteTime });
				}
				foreach (var directoryInfo in rootDirectoryInfo.EnumerateDirectories()) {
					if (scanFilter?.Invoke(directoryInfo) ?? true) {
						queue.Enqueue(directoryInfo);
					}
				}
			}
#if MAC
			// Mono 6.0 breaks files order
			files.Sort ((a, b) => string.Compare (a.Path, b.Path));
#endif
		}

		public IEnumerable<FileInfo> Enumerate(string extension = null)
		{
			if (extension == null && EnumerationFilter == null) {
				return files;
			}
			return files
				.Where(file => extension == null || file.Path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
				.Where(file => EnumerationFilter == null || EnumerationFilter(file));
		}
	}
}
