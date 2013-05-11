using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lime
{
	public class UnpackedAssetsBundle : AssetsBundle
	{
		public readonly string BaseDirectory;

		public UnpackedAssetsBundle(string baseDirectory)
		{
			this.BaseDirectory = baseDirectory;
		}

		public override Stream OpenFile(string path)
		{
			return new FileStream(Path.Combine(BaseDirectory, path), FileMode.Open);
		}

		public override DateTime GetFileLastWriteTime(string path)
		{
#if UNITY_WEBPLAYER
			throw new NotImplementedException();
#else
			return File.GetLastWriteTime(Path.Combine(BaseDirectory, path));
#endif
		}

		public override void DeleteFile(string path)
		{
			File.Delete(Path.Combine(BaseDirectory, path));
		}

		public override bool FileExists(string path)
		{
			return File.Exists(Path.Combine(BaseDirectory, path));
		}

		public override void ImportFile(string path, Stream stream, int reserve, bool compress)
		{
#if UNITY_WEBPLAYER
			throw new NotImplementedException();
#else
			stream.Seek(0, SeekOrigin.Begin);
			var bytes = new byte[stream.Length];
			stream.Read(bytes, 0, bytes.Length);
			File.WriteAllBytes(Path.Combine(BaseDirectory, path), bytes);
#endif
		}

		public override string[] EnumerateFiles()
		{
			throw new NotImplementedException();
		}
	}
}
