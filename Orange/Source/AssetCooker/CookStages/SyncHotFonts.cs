using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncHotFonts : CookStage
	{
		public override IEnumerable<string> ImportedExtensions { get { yield return hotFontExtension; } }
		public override IEnumerable<string> BundleExtensions { get { yield return hotFontExtension; } }

		private readonly string hotFontExtension = ".fnt";

		public override void Action()
		{
			SyncUpdated.Sync(hotFontExtension, hotFontExtension, AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			var importer = new HotFontImporter(false);
			var font = importer.ParseFont(srcPath, dstPath);
			Serialization.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, font, Serialization.Format.Binary, hotFontExtension, File.GetLastWriteTime(srcPath),
				AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
