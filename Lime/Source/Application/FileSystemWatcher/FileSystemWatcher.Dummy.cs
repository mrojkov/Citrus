#if !WIN && !MAC
using System;

namespace Lime
{
	public class FileSystemWatcher : IFileSystemWatcher
	{
		public event Action<string> Changed;
		public event Action<string> Created;
		public event Action<string> Deleted;
		public event Action<string, string> Renamed;

		public FileSystemWatcher(string path, bool includeSubdirectories)
		{
		}

		public void Dispose()
		{
		}
	}
}
#endif
