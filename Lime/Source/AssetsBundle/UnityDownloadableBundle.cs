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
		private string prefix;

		public UnityDownloadableBundle(UnityEngine.AssetBundle bundle)
		{
			this.prefix = "Assets/Bundles/" + bundle.name + "/";
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
			path = LimePathToUnityPath(path);
			var result = bundle.LoadAsset(path) as T;
			if (result == null) {
				throw new Lime.Exception("Asset not found: {0}", path);
			}
			return result;
		}

		private string LimePathToUnityPath(string path)
		{
			return prefix + GetAssetPathWithRightExtension(path);
		}

		private string GetAssetPathWithRightExtension(string path)
		{
			var ext = Path.GetExtension(path);
			if (ext != ".png" && ext != ".txt" && ext != ".ogg") {
				path += ".bytes";
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