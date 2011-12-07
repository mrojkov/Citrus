using System;
using System.Collections.Generic;
using System.IO;

namespace Lime
{
	internal class SampleCache
	{
		public const int MaxCachedSoundSize = 128 * 1024;
		public const int SoundCacheSize = 32;

		struct CacheItem
		{
			public string Path;
			public byte [] Buffer;
		}

		List<CacheItem> items = new List<CacheItem> ();

		public Stream OpenStream (string path)
		{
			for (int i = 0; i < items.Count; i++) {
				var item = items [i];
				if (item.Path == path) {
					items.RemoveAt (i);
					items.Add (item);
					return new MemoryStream (item.Buffer);
				}
			}
			var stream = AssetsBundle.Instance.OpenFile (path);
			if (stream.Length < MaxCachedSoundSize) {
				var stream2 = new MemoryStream ((int)stream.Length);
				stream.CopyTo (stream2);
				stream2.Position = 0;
				if (items.Count >= SoundCacheSize) {
					items.RemoveAt (0);
				}
				items.Add (new CacheItem {Buffer = stream2.GetBuffer(), Path = path});
				return stream2;
			}
			return stream;
		}
	}}
