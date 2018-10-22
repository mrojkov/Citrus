using System;

namespace Lime
{
	public interface IFileSystemWatcher : IDisposable
	{
		event Action<string> Changed;
		event Action<string> Created;
		event Action<string> Deleted;
		event Action<string, string> Renamed;
	}
}
