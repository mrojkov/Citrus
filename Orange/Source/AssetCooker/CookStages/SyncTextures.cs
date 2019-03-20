using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Lime;

namespace Orange
{
	class SyncTextures: CookStage
	{
		public SyncTextures() : base()
		{

		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".png" };
		}

		public override void Action()
		{
			SyncUpdated.Sync(".png", AssetCooker.GetPlatformTextureExtension(), AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			var rules = AssetCooker.CookingRulesMap[Path.ChangeExtension(dstPath, ".png")];
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
