using System.IO;
using Lime;

namespace Orange
{
	class SyncHotScenes : CookStage
	{
		public SyncHotScenes() : base()
		{

		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".scene" };
		}

		public override void Action()
		{
			SyncUpdated.Sync(".scene", ".scene", AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			using (Stream stream = new FileStream(srcPath, FileMode.Open)) {
				var node = new HotSceneImporter(false, srcPath).Import(stream, null, null);
				Serialization.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, node, Serialization.Format.Binary, ".scene",
					File.GetLastWriteTime(srcPath), AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
			}
			return true;
		}
	}
}
