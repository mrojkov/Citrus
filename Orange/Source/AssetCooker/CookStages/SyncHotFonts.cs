using System.IO;
using Lime;

namespace Orange
{
	class SyncHotFonts : CookStage
	{
		public SyncHotFonts() : base()
		{

		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".fnt" };
			ImportedExtension = Extensions[0];
			ExportedExtension = Extensions[0];
		}

		public override void Action()
		{
			SyncUpdated.Sync(ImportedExtension, ExportedExtension, AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			var importer = new HotFontImporter(false);
			var font = importer.ParseFont(srcPath, dstPath);
			Serialization.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, font, Serialization.Format.Binary, ExportedExtension, File.GetLastWriteTime(srcPath),
				AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
