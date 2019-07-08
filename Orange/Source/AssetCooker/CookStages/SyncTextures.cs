using System;
using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncTextures: ICookStage
	{
		public IEnumerable<string> ImportedExtensions { get { yield return originalTextureExtension; } }
		public IEnumerable<string> BundleExtensions { get { yield return PlatformTextureExtension; } }

		private readonly string originalTextureExtension = ".png";
		private string PlatformTextureExtension => AssetCooker.GetPlatformTextureExtension();

		public int GetOperationsCount() => SyncUpdated.GetOperationsCount(originalTextureExtension);

		public void Action(Target target) => SyncUpdated.Sync(target, originalTextureExtension, PlatformTextureExtension, AssetBundle.Current, Converter);

		private bool Converter(Target target, string srcPath, string dstPath)
		{
			var rules = AssetCooker.CookingRulesMap[Path.ChangeExtension(dstPath, originalTextureExtension)];
			if (rules.TextureAtlas != null) {
				// No need to cache this texture since it is a part of texture atlas.
				return false;
			}
			using (var stream = File.OpenRead(srcPath)) {
				var bitmap = new Bitmap(stream);
				if (TextureTools.ShouldDownscale(bitmap, rules)) {
					var scaledBitmap = TextureTools.DownscaleTexture(bitmap, srcPath, rules);
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
