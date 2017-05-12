using System.Collections.Generic;

namespace Orange
{
	public interface IFileEnumerator
	{
		string Directory { get; }
		List<FileInfo> Enumerate(string extension = null);
		void Rescan();
	}
}