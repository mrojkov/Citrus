using System;
using System.Collections.Generic;

namespace Orange
{
	public interface IFileEnumerator
	{
		string Directory { get; }
		Predicate<FileInfo> EnumerationFilter { get; set; }
		List<FileInfo> Enumerate(string extension = null);
		void Rescan();
	}
}