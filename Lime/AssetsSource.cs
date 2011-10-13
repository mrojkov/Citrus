using System;
using System.Collections.Generic;
using System.IO;

namespace Lime
{
	public struct AssetsSourceSwitcher : IDisposable
	{
		public AssetsSourceSwitcher (IAssetsSource source)
		{
			AssetsSource.Current = source;
		}
		
		public void Dispose () 
		{
		}
	}
	
	public static class AssetsSource
	{
		public static IAssetsSource Current;
		
		public static AssetsSourceSwitcher AssetsFolder ()
		{
			return new AssetsSourceSwitcher ();
		}
	}
	
	public interface IAssetsSource
	{
		Stream OpenFile (string path);
		DateTime GetFileModificationTime (string path);
		void RemoveFile (string path);
		bool FileExists (string path);
		bool FileExists (string path, string extension);
		ICollection<string> EnumerateFiles ();
	}
}

