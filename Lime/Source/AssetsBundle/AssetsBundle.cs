using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lime
{
	public abstract class AssetsBundle : IDisposable
	{
		private static AssetsBundle instance;

		const float iPadFlashReadingSpeed = 50 * 1024 * 1024;

		public static AssetsBundle Instance
		{
			get
			{
				if (instance == null) {
					throw new Lime.Exception("AssetsBundle.Instance should initialized before usage");
				}
				return instance;
			}
			set { instance = value; }
		}

		public virtual void Dispose()
		{
			if (instance == this) {
				instance = null;
			}
		}

		public string CurrentLanguage { get; set; }

		public abstract Stream OpenFile(string path);
		public abstract DateTime GetFileLastWriteTime(string path);
		public abstract void DeleteFile(string path);
		public abstract bool FileExists(string path);
		public abstract void ImportFile(string path, Stream stream, int reserve, bool compress = false);
		public abstract string[] EnumerateFiles();

		public void ImportFile(string srcPath, string dstPath, int reserve, bool compress = false)
		{
			using (var stream = new FileStream(srcPath, FileMode.Open)) {
				ImportFile(dstPath, stream, reserve, compress);
			}
		}

		public Stream OpenFileLocalized(string path)
		{
			var stream = OpenFile(GetLocalizedPath(path));
			if (Application.CheckCommandLineArg("--iPadLags")) {
				int readTime = (int)(1000 * stream.Length / iPadFlashReadingSpeed);
				System.Threading.Thread.Sleep(readTime);
				if (readTime > 20) {
					Console.WriteLine("Lag {0} ms while reading {1}", readTime, path);
				}
			}
			return stream;
		}

		public string GetLocalizedPath(string path)
		{
			if (string.IsNullOrEmpty(CurrentLanguage))
				return path;
			string extension = Path.GetExtension(path);
			string pathWithoutExtension = Path.ChangeExtension(path, null);
			string localizedParth = pathWithoutExtension + "." + CurrentLanguage + extension;
			if (FileExists(localizedParth)) {
				return localizedParth;
			}
			return path;
		}
	}
}
