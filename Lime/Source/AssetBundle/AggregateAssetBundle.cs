using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Lime
{
	public class AggregateAssetBundle : AssetBundle
	{
		private readonly List<AssetBundle> bundles = new List<AssetBundle>();
		private ReaderWriterLockSlim sync = new ReaderWriterLockSlim();

		public AggregateAssetBundle(params AssetBundle[] bundles)
		{
			sync.EnterWriteLock();
			try {
				this.bundles.AddRange(bundles);
			} finally {
				sync.ExitWriteLock();
			}
		}

		public void Attach(AssetBundle bundle)
		{
			sync.EnterWriteLock();
			try {
				bundles.Add(bundle);
			} finally {
				sync.ExitWriteLock();
			}
		}

		public void Detach(AssetBundle bundle)
		{
			sync.EnterWriteLock();
			try {
				bundles.Remove(bundle);
			} finally {
				sync.ExitWriteLock();
			}
		}

		public override Stream OpenFile(string path)
		{
			sync.EnterReadLock();
			try {
				foreach (var bundle in bundles) {
					if (bundle.FileExists(path)) {
						return bundle.OpenFile(path);
					}
				}
			} finally {
				sync.ExitReadLock();
			}
			throw new FileNotFoundException($"File {path} not found in aggregate asset bundle.");
		}

		public override void Dispose()
		{
			sync.EnterWriteLock();
			try {
				foreach (var bundle in bundles) {
					bundle.Dispose();
				}
				bundles.Clear();
			} finally {
				sync.ExitWriteLock();
			}
		}

		public override DateTime GetFileLastWriteTime(string path)
		{
			sync.EnterReadLock();
			try {
				foreach (var bundle in bundles) {
					if (bundle.FileExists(path)) {
						return bundle.GetFileLastWriteTime(path);
					}
				}
			} finally {
				sync.ExitReadLock();
			}
			throw new InvalidOperationException($"Path {path} not found in aggregate asset bundle.");
		}

		public override byte[] GetCookingRulesSHA1(string path)
		{
			sync.EnterReadLock();
			try {
				foreach (var bundle in bundles) {
					if (bundle.FileExists(path)) {
						return bundle.GetCookingRulesSHA1(path);
					}
				}
			} finally {
				sync.ExitReadLock();
			}
			throw new InvalidOperationException($"Path {path} not found in aggregate asset bundle.");
		}

		public override int GetFileSize(string path)
		{
			sync.EnterReadLock();
			try {
				foreach (var bundle in bundles) {
					if (bundle.FileExists(path)) {
						return bundle.GetFileSize(path);
					}
				}
			} finally {
				sync.ExitReadLock();
			}
			throw new InvalidOperationException($"Path {path} not found in aggregate asset bundle.");
		}

		public override void DeleteFile(string path)
		{
			throw new InvalidOperationException("Not supported by aggregate asset bundle.");
		}

		public override bool FileExists(string path)
		{
			sync.EnterReadLock();
			try {
				foreach (var bundle in bundles) {
					if (bundle.FileExists(path)) {
						return true;
					}
				}
			} finally {
				sync.ExitReadLock();
			}
			return false;
		}

		public override void ImportFile(string path, Stream stream, int reserve, string sourceExtension, DateTime time,
			AssetAttributes attributes = AssetAttributes.None, byte[] cookingRulesSHA1 = null)
		{
			throw new InvalidOperationException("Not supported by aggregate asset bundle.");
		}

		public override IEnumerable<string> EnumerateFiles(string path = null)
		{
			sync.EnterReadLock();
			try {
				return bundles.SelectMany(bundle => bundle.EnumerateFiles(path));
			} finally {
				sync.ExitReadLock();
			}
		}
	}
}
