#if MAC
using System;
using CoreServices;
using Foundation;

namespace Lime
{
	public class FileSystemWatcher : IFileSystemWatcher
	{
		private FSEventStream fsEventStream;

		public event Action<string> Changed;
		public event Action<string> Created;
		public event Action<string> Deleted;
		public event Action<string, string> Renamed;

		private Func<string, bool> Filter;

		public FileSystemWatcher(string path, bool includeSubdirectories)
		{
			if (!includeSubdirectories) {
				Filter = (s) => {
					return System.IO.Path.GetDirectoryName(s) == path;
				};
			}
			fsEventStream = new FSEventStream(
				new string[] { path }, TimeSpan.FromSeconds(1),
				FSEventStreamCreateFlags.FileEvents | FSEventStreamCreateFlags.IgnoreSelf);
			fsEventStream.Events += handleEvents;
			fsEventStream.ScheduleWithRunLoop(NSRunLoop.Current);
			fsEventStream.Start();
		}

		private void handleEvents(object sender, FSEventStreamEventsArgs args)
		{
			foreach (var e in args.Events) {
				if ((e.Flags & FSEventStreamEventFlags.ItemModified) != 0) {
					if (Filter?.Invoke(e.Path) ?? true) {
						Changed?.Invoke(e.Path);
					}
				}
				if ((e.Flags & FSEventStreamEventFlags.ItemCreated) != 0) {
					if (Filter?.Invoke(e.Path) ?? true) {
						Changed?.Invoke(e.Path);
					}
				}
				if ((e.Flags & FSEventStreamEventFlags.ItemRemoved) != 0) {
					if (Filter?.Invoke(e.Path) ?? true) {
						Changed?.Invoke(e.Path);
					}
				}
				if ((e.Flags & FSEventStreamEventFlags.ItemRenamed) != 0) {
					if (Filter?.Invoke(e.Path) ?? true) {
						Changed?.Invoke(/* TODO: pass previous path before rename here */, e.Path);
					}
				}
			}
		}

		public void Dispose()
		{
			if (fsEventStream != null) {
				fsEventStream.Dispose();
				fsEventStream = null;
			}
		}
	}
}
#endif
