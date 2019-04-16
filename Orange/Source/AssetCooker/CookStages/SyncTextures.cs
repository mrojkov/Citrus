using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncTextures: CookStage
	{
		public override IEnumerable<string> ImportedExtensions { get { yield return originalTextureExtension; } }
		public override IEnumerable<string> BundleExtensions { get { yield return platformTextureExtension; } }

		private readonly string originalTextureExtension = ".png";
		private readonly string platformTextureExtension = AssetCooker.GetPlatformTextureExtension();

		public override void Action()
		{
			SyncUpdated.Sync(originalTextureExtension, platformTextureExtension, AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			var rules = AssetCooker.CookingRulesMap[Path.ChangeExtension(dstPath, originalTextureExtension)];
			if (rules.TextureAtlas != null) {
				// Reverse double counting
				UserInterface.Instance.IncreaseProgressBar(-1);
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
