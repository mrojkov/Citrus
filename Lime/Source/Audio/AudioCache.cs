using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Lime
{
	internal class AudioCache
	{
		public const int MaxCachedSoundSize = 128 * 1024;
		public const int SoundCacheSize = 32;
		private List<Item> items = new List<Item>();

		class Item
		{
			public string Path;
			public byte[] Buffer;
		}

		public void OpenStreamAsync(string path, Action<Stream> succeeded)
		{
			var stream = GetCachedStream(path);
			if (stream != null) {
				succeeded(stream);
				return;
			}
			stream = PackedAssetsBundle.Instance.OpenFileLocalized(path);
			if (stream.Length >= MaxCachedSoundSize) {
				succeeded(stream);
			} else {
				ReadoutAndCacheStreamAsync(path, stream, succeeded);
			}
		}

		private void ReadoutAndCacheStreamAsync(string path, Stream stream, Action<Stream> succeeded)
		{
			var stream2 = new MemoryStream((int)stream.Length);
			var bw = new BackgroundWorker();
			bw.DoWork += (s, e) => {
				stream.CopyTo(stream2);
			};
			bw.RunWorkerCompleted += (s, e) => {
				stream2.Position = 0;
				if (items.Count >= SoundCacheSize) {
					items.RemoveAt(0);
				}
				items.Add(new Item { Buffer = stream2.GetBuffer(), Path = path });
				succeeded(stream2);
			};
			bw.RunWorkerAsync();
		}

		//public Stream OpenStream(string path)
		//{
		//	var stream = GetCachedStream(path);
		//	if (stream != null) {
		//		return stream;
		//	}
		//	stream = PackedAssetsBundle.Instance.OpenFileLocalized(path);
		//	if (stream.Length < MaxCachedSoundSize) {
		//		var stream2 = new MemoryStream((int)stream.Length);
		//		stream.CopyTo(stream2);
		//		stream2.Position = 0;
		//		if (items.Count >= SoundCacheSize) {
		//			items.RemoveAt(0);
		//		}
		//		items.Add(new CacheItem {
		//			Buffer = stream2.GetBuffer(), 
		//			Path = path
		//		});
		//		return stream2;
		//	}
		//	return stream;
		//}

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
