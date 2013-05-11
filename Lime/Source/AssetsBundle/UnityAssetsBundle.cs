#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lime
{
	public sealed class UnityAssetsBundle : AssetsBundle
	{
		List<string> fileList;

		public new static UnityAssetsBundle Instance
		{
			get { return AssetsBundle.Instance as UnityAssetsBundle; }
		}

		public UnityAssetsBundle()
		{
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

		private string GetAssetPathWOExtension(string path)
		{
			var ext = Path.GetExtension(path);
			if (ext == ".png" || ext == ".txt" || ext == ".ogg") {
				path = System.IO.Path.ChangeExtension(path, null);
			}
			return path;
		}

		public T LoadUnityAsset<T>(string path) where T : UnityEngine.Object
		{
			path = GetAssetPathWOExtension(path);
			var result = UnityEngine.Resources.Load(path, typeof(T)) as T;
			if (result == null) {
				throw new Lime.Exception("Asset not found: {0}", path);
			}
			return result;
		}

		public void UnloadUnityAsset(UnityEngine.Object asset)
		{
			UnityEngine.Resources.UnloadAsset(asset);
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

		public override void ImportFile(string path, Stream stream, int reserve, bool compress)
		{
			throw new NotImplementedException();
		}

		public override string[] EnumerateFiles()
		{
			return fileList.ToArray();
		}
	}
}
#endif