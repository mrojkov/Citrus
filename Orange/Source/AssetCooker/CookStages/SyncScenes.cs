using System.IO;
using Lime;

namespace Orange
{
	class SyncScenes : CookStage
	{
		public SyncScenes() : base()
		{

		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".tan" };
			ImportedExtension = Extensions[0];
			ExportedExtension = Extensions[0];
		}

		public override void Action()
		{
			SyncUpdated.Sync(ImportedExtension, ExportedExtension, AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			var node = Serialization.ReadObjectFromFile<Node>(srcPath);
			Serialization.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, node, Serialization.Format.Binary, ExportedExtension,
				File.GetLastWriteTime(srcPath), AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
