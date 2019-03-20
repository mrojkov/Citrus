using System.IO;
using Lime;

namespace Orange
{
	class SyncFonts: CookStage
	{
		public SyncFonts() : base()
		{

		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".tft" };
		}

		public override void Action()
		{
			SyncUpdated.Sync(".tft", ".tft", AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			var font = Serialization.ReadObjectFromFile<Font>(srcPath);
			Serialization.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, font, Serialization.Format.Binary, ".tft",
				File.GetLastWriteTime(srcPath), AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
