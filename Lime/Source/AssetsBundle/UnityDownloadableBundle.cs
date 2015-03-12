#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lime
{
	public sealed class UnityDownloadableBundle : AssetsBundle
	{
		private List<string> fileList;
		private UnityEngine.AssetBundle bundle;

		public UnityDownloadableBundle(UnityEngine.AssetBundle bundle)
		{
			this.bundle = bundle;
			ReadFileList();
		}

		private void ReadFileList()
		{
			fileList = new List<string>();
			using (var reader = new StreamReader(OpenFile("Files.txt"))) {
				while (!reader.EndOfStream) {
					fileList.Add(reader.ReadLine());
				}
			}
		}

		public override T LoadUnityAsset<T>(string path)
		{
			path = GetAssetPathWOExtension(path);
			var result = bundle.Load(path, typeof(T)) as T;
			if (result == null) {
				throw new Lime.Exception("Asset not found: {0}", path);
			}
			return result;
		}

		private string GetAssetPathWOExtension(string path)
		{
			var ext = Path.GetExtension(path);
			if (ext == ".png" || ext == ".txt" || ext == ".ogg") {
				path = System.IO.Path.ChangeExtension(path, null);
			}
			return path;
		}

		public override Stream OpenFile(string path)
		{
			var asset = LoadUnityAsset<UnityEngine.TextAsset>(path);
			return new MemoryStream(asset.bytes);
		}

		public override DateTime GetFileLastWriteTime(string path)
		{
			throw new NotImplementedException();
		}

		public override void DeleteFile(string path)
		{
			throw new NotImplementedException();
		}

		public override bool FileExists(string path)
		{
			return fileList.Contains(path);
		}

		public override void ImportFile(string path, Stream stream, int reserve, AssetAttributes attributes)
		{
			throw new NotImplementedException();
		}
		
		public override IEnumerable<string> EnumerateFiles()
		{
			return fileList.ToArray();
		}
	}
}
#endif