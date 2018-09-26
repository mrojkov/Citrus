using System.Collections.Generic;
using System.IO;
using Lime;
using Yuzu;

namespace Tangerine.Core
{
	public class TangerineAssetBundle : UnpackedAssetBundle
	{
		private const string VersionFile = "__CACHE_VERSION__";

		public class CacheMeta
		{
			private const string CurrentVersion = "1.2";

			[YuzuRequired]
			public string Version { get; set; } = CurrentVersion;

			public bool IsActual => Version == CurrentVersion;
		}

		public TangerineAssetBundle(string baseDirectory) : base(baseDirectory) { }

		public bool IsActual()
		{
			using (var cacheBundle = new PackedAssetBundle(Orange.The.Workspace.TangerineCacheBundle, AssetBundleFlags.Writable)) {
				if (!cacheBundle.FileExists(VersionFile)) {
					return false;
				}
				try {
					using (var stream = cacheBundle.OpenFile(VersionFile)) {
						var cacheMeta = TangerineYuzu.Instance.Value.ReadObject<CacheMeta>(VersionFile, stream);
						if (!cacheMeta.IsActual) {
							return false;
						}
					}
				} catch {
					return false;
				}
			}
			return true;
		}

		public void CleanupBundle()
		{
			using (var cacheBundle = new PackedAssetBundle(Orange.The.Workspace.TangerineCacheBundle, AssetBundleFlags.Writable)) {
				foreach (var path in cacheBundle.EnumerateFiles()) {
					cacheBundle.DeleteFile(path);
				}
				TangerineYuzu.Instance.Value.WriteObjectToBundle(cacheBundle, VersionFile, new CacheMeta(), Serialization.Format.Binary, string.Empty, AssetAttributes.None, new byte[0]);
			}
		}

		public override Stream OpenFile(string path)
		{
			if (Path.GetExtension(path) != ".t3d") {
				return base.OpenFile(path);
			}
			var exists3DScene = base.FileExists(path);
			var fbxPath = Path.ChangeExtension(path, "fbx");
			var existsFbx = base.FileExists(fbxPath);
			if (existsFbx && exists3DScene) {
				throw new Exception($"Ambiguity between: {path} and {fbxPath}");
			}
			return exists3DScene ? base.OpenFile(path) : OpenFbx(path);
		}

		private Stream OpenFbx(string path)
		{
			using (var cacheBundle = new PackedAssetBundle(Orange.The.Workspace.TangerineCacheBundle, AssetBundleFlags.Writable)) {
				var fbxPath = Path.ChangeExtension(path, "fbx");
				if (!cacheBundle.FileExists(path) || GetFileLastWriteTime(fbxPath) > cacheBundle.GetFileLastWriteTime(path)) {
					var fullPath = Path.Combine(Orange.The.Workspace.AssetsDirectory, fbxPath);
					var model = new Orange.FbxModelImporter(fullPath, Orange.The.Workspace.ActiveTarget, new Dictionary<string, Orange.CookingRules>()).Model;
					TangerineYuzu.Instance.Value.WriteObjectToBundle(cacheBundle, path, model, Serialization.Format.Binary, ".t3d", AssetAttributes.None, new byte[0]);
				}
			}
			using (var cacheBundle = new PackedAssetBundle(Orange.The.Workspace.TangerineCacheBundle)) {
				return cacheBundle.OpenFile(path);
			}
		}

		public override bool FileExists(string path)
		{
			if (Path.GetExtension(path) != ".t3d") {
				return base.FileExists(path);
			}
			var exists3DScene = base.FileExists(path);
			var fbxPath = Path.ChangeExtension(path, "fbx");
			var existsFbx = base.FileExists(fbxPath);
			if (existsFbx && exists3DScene) {
				throw new Exception($"Ambiguity between: {path} and {fbxPath}");
			}
			return exists3DScene || existsFbx;
		}
	}
}
