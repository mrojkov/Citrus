using System;
using System.Collections.Generic;
using System.IO;

namespace Lime
{
	public struct AssetsSourceSwitcher : IDisposable
	{
		public AssetsSourceSwitcher(IAssetsSource source)
		{
			AssetsSource.Current = source;
		}
		
		public void Dispose()
		{
		}
	}
	
	public static class AssetsSource
	{
		public static IAssetsSource Current;
		
		public static AssetsSourceSwitcher AssetsFolder()
		{
			return new AssetsSourceSwitcher();
		}
	}
	
	public interface IAssetsSource
	{
		Stream OpenFile(string path);

		DateTime GetFileLastWriteTime(string path);

		void DeleteFile(string path);

		bool FileExists(string path);
		
		string [] EnumerateFiles();
	}
}

