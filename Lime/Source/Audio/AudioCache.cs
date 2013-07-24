using System;
using System.Collections.Generic;
using System.IO;

namespace Lime
{
	internal class AudioCache
	{
		class CachedSample
		{
			public string Path;
			public byte[] Data;
		}

		public const int MaxCachedSampleSize = 256 * 1024;
		public const int MaxCacheSize = 4 * 1024 * 1024;
		public int CacheSize { get { return CalcCacheSize(); } }

		private List<CachedSample> samples = new List<CachedSample>();
		private object sync = new object();

		private int CalcCacheSize()
		{
			int size = 0;
			lock (sync) {
				foreach (var i in samples) {
					size += i.Data.Length;
				}
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
			if (stream.Length < MaxCachedSampleSize) {
				var memStream = new MemoryStream((int)stream.Length);
				stream.CopyTo(memStream);
				memStream.Position = 0;
				while (CalcCacheSize() > MaxCacheSize) {
					lock (sync) {
						samples.RemoveAt(0);
					}
				}
				lock (sync) {
					samples.Add(new CachedSample {
						Data = memStream.GetBuffer(),
						Path = path
					});
				}
				return memStream;
			}
			return stream;
		}

		public bool IsSampleCached(string path)
		{
			lock (sync) {
				return samples.Find(i => i.Path == path) != null;
			}
		}

		private Stream GetCachedStream(string path)
		{
			lock (sync) {
				for (int i = 0; i < samples.Count; i++) {
					var sample = samples[i];
					if (sample.Path == path) {
						samples.RemoveAt(i);
						samples.Add(sample);
						return new MemoryStream(sample.Data);
					}
				}
			}
			return null;
		}
	}
}
