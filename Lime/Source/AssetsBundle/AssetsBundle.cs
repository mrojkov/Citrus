using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lime
{
	public abstract class AssetsBundle
	{
		private static AssetsBundle instance;

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

		public string CurrentLanguage { get; set; }

		public abstract Stream OpenFile(string path);
		public abstract DateTime GetFileLastWriteTime(string path);
		public abstract void DeleteFile(string path);
		public abstract bool FileExists(string path);
		public abstract void ImportFile(string path, Stream stream, int reserve);
		public abstract string[] EnumerateFiles();

		public void ImportFile(string srcPath, string dstPath, int reserve)
		{
			using (var stream = new FileStream(srcPath, FileMode.Open)) {
				ImportFile(dstPath, stream, reserve);
			}
		}

		public Stream OpenFileLocalized(string path)
		{
			return OpenFile(GetLocalizedPath(path));
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
