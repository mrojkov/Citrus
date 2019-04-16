using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncFonts: CookStage
	{
		public override IEnumerable<string> ImportedExtensions { get { yield return fontExtension; } }
		public override IEnumerable<string> BundleExtensions { get { yield return fontExtension; } }

		private readonly string fontExtension = ".tft";

		public override void Action()
		{
			SyncUpdated.Sync(fontExtension, fontExtension, AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			var font = Serialization.ReadObjectFromFile<Font>(srcPath);
			Serialization.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, font, Serialization.Format.Binary, fontExtension,
				File.GetLastWriteTime(srcPath), AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
