#if OPENAL
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

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

		private readonly List<CachedSample> samples = new List<CachedSample>();
		private readonly object sync = new object();

		private int CalcCacheSize()
		{
			lock (sync) {
				return samples.Sum(i => i.Data.Length);
			}
		}

		public Stream OpenStream(string path)
		{
			var stream = GetCachedStream(path);
			if (stream != null) {
				return stream;
			}
			if (!AssetBundle.Current.FileExists(path)) {
				Logger.Write("Missing audio file '{0}'", path);
				return null;
			}
			stream = PackedAssetBundle.Current.OpenFileLocalized(path);
			if (stream.Length < MaxCachedSampleSize) {
				CacheSoundAsync(path);
			}
			return stream;
		}

		private void CacheSoundAsync(string path)
		{
			var assetBundle = AssetBundle.Current;
			var bw = new BackgroundWorker();
			bw.DoWork += (s, e) => {
				using (var stream = assetBundle.OpenFileLocalized(path)) {
					var memStream = new MemoryStream((int)stream.Length);
					stream.CopyTo(memStream);
					memStream.Position = 0;
					Cleanup();
					lock (sync) {
						if (samples.FindIndex(i => i.Path == path) < 0) {
							samples.Add(new CachedSample {
								Data = memStream.GetBuffer(),
								Path = path
							});
						}
					}
				}
			};
			bw.RunWorkerAsync();
		}

		private void Cleanup()
		{
			while (CalcCacheSize() > MaxCacheSize) {
				lock (sync) {
					samples.RemoveAt(0);
				}
			}
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
#endif