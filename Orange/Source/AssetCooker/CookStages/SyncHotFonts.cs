using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncHotFonts : AssetCookerCookStage, ICookStage
	{
		public IEnumerable<string> ImportedExtensions { get { yield return hotFontExtension; } }
		public IEnumerable<string> BundleExtensions { get { yield return hotFontExtension; } }

		private readonly string hotFontExtension = ".fnt";

		public SyncHotFonts(AssetCooker assetCooker) : base(assetCooker) { }

		public int GetOperationsCount() => SyncUpdated.GetOperationsCount(hotFontExtension);

		public void Action() => SyncUpdated.Sync(hotFontExtension, hotFontExtension, AssetBundle.Current, Converter);

		private bool Converter(string srcPath, string dstPath)
		{
			var importer = new HotFontImporter(false);
			var font = importer.ParseFont(srcPath, dstPath);
			InternalPersistence.Instance.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, font, Persistence.Format.Binary, hotFontExtension, File.GetLastWriteTime(srcPath),
				AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
