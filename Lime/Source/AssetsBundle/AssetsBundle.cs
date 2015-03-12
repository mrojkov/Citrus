using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lime
{
	[Flags]
	public enum AssetAttributes
	{
		None = 0,
		Zipped = 1 << 0,
		NonPowerOf2Texture = 1 << 1
	}

	public abstract class AssetsBundle : IDisposable
	{
		private static AssetsBundle instance;

		public static AssetsBundle Instance
		{
			get { return GetInstance(); }
			set { instance = value; }
		}

		private static AssetsBundle GetInstance()
		{
			if (instance == null) {
				throw new Lime.Exception("AssetsBundle.Instance should be initialized before the usage");
			}
			return instance;
		}

		public virtual void Dispose()
		{
			if (instance == this) {
				instance = null;
			}
		}

		public static string CurrentLanguage;

		public abstract Stream OpenFile(string path);
		public abstract DateTime GetFileLastWriteTime(string path);
		public abstract void DeleteFile(string path);
		public abstract bool FileExists(string path);
		public abstract void ImportFile(string path, Stream stream, int reserve, AssetAttributes attributes = AssetAttributes.None);
		public abstract IEnumerable<string> EnumerateFiles();

		public void ImportFile(string srcPath, string dstPath, int reserve, AssetAttributes attributes = AssetAttributes.None)
		{
			using (var stream = new FileStream(srcPath, FileMode.Open)) {
				ImportFile(dstPath, stream, reserve, attributes);
			}
		}

		public Stream OpenFileLocalized(string path)
		{
			var stream = OpenFile(GetLocalizedPath(path));
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

#if UNITY
		public virtual T LoadUnityAsset<T>(string path) where T : UnityEngine.Object
		{
			throw new NotImplementedException();
		}
#endif

		public virtual AssetAttributes GetAttributes(string path)
		{
			return AssetAttributes.None;
		}

		public virtual void SetAttributes(string path, AssetAttributes attributes)
		{
		}
	}
}
