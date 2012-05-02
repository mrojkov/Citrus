using System;
using System.IO;

namespace Orange
{
	public struct DirectoryChanger : IDisposable
	{
		private string oldDirectory;
	
		public DirectoryChanger(string directory)
		{
			oldDirectory = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(directory);
		}
		
		public void Dispose()
		{
			Directory.SetCurrentDirectory(oldDirectory);
		}
	}
}