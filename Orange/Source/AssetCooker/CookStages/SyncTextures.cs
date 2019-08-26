using System;
using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncTextures : AssetCookerCookStage, ICookStage
	{
		public IEnumerable<string> ImportedExtensions { get { yield return originalTextureExtension; } }
		public IEnumerable<string> BundleExtensions { get { yield return PlatformTextureExtension; } }

		private readonly string originalTextureExtension = ".png";

		public SyncTextures(AssetCooker assetCooker) : base(assetCooker) { }

		private string PlatformTextureExtension => AssetCooker.GetPlatformTextureExtension();

		public int GetOperationsCount() => SyncUpdated.GetOperationsCount(originalTextureExtension);

		public void Action() => SyncUpdated.Sync(originalTextureExtension, PlatformTextureExtension, AssetBundle.Current, Converter);

		private bool Converter(string srcPath, string dstPath)
		{
			var rules = AssetCooker.CookingRulesMap[Path.ChangeExtension(dstPath, originalTextureExtension)];
			if (rules.TextureAtlas != null) {
				// No need to cache this texture since it is a part of texture atlas.
				return false;
			}
			using (var stream = File.OpenRead(srcPath)) {
				var bitmap = new Bitmap(stream);
				if (TextureTools.ShouldDownscale(AssetCooker.Platform, bitmap, rules)) {
					var scaledBitmap = TextureTools.DownscaleTexture(AssetCooker.Platform, bitmap, srcPath, rules);
					bitmap.Dispose();
					bitmap = scaledBitmap;
				}
				AssetCooker.ImportTexture(dstPath, bitmap, rules, File.GetLastWriteTime(srcPath), rules.SHA1);
				bitmap.Dispose();
			}
			return true;
		}
	}
}
