using System;
using System.Collections.Generic;
using System.IO;

namespace Lime
{
	internal class AudioCache
	{
		public const int MaxCachedSoundSize = 256 * 1024;
		public const int MaxCacheSize = 4 * 1024 * 1024;
		private List<Item> items = new List<Item>();

		class Item
		{
			public string Path;
			public byte[] Buffer;
		}

		public int CalcCacheSize()
		{
			int size = 0;
			foreach (var i in items) {
				size += i.Buffer.Length;
			}
			return size;
		}

		public Stream OpenStream(string path)
		{
			var stream = GetCachedStream(path);
			if (stream != null) {
				return stream;
			}
			if (!AssetsBundle.Instance.FileExists(path)) {
				Logger.Write("Missing audio file '{0}'", path);
				return null;
			}
			stream = PackedAssetsBundle.Instance.OpenFileLocalized(path);
			if (stream.Length < MaxCachedSoundSize) {
				Logger.Write("Caching sound {0}", path);
				var memStream = new MemoryStream((int)stream.Length);
				stream.CopyTo(memStream);
				memStream.Position = 0;
				while (CalcCacheSize() > MaxCacheSize) {
					items.RemoveAt(0);
				}
				items.Add(new Item {
					Buffer = memStream.GetBuffer(),
					Path = path
				});
				return memStream;
			}
			return stream;
		}

		private Stream GetCachedStream(string path)
		{
			for (int i = 0; i < items.Count; i++) {
				var item = items[i];
				if (item.Path == path) {
					items.RemoveAt(i);
					items.Add(item);
					return new MemoryStream(item.Buffer);
				}
			}
			return null;
		}
	}
}
