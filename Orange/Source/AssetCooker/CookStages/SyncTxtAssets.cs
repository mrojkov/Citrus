using System.IO;
using Lime;

namespace Orange
{
	class SyncTxtAssets: CookStage
	{
		public SyncTxtAssets(): base()
		{

		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".txt", ".t3d" };
		}

		public override void Action()
		{
			SyncUpdated.Sync(".txt", ".txt", AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			var modelAttachmentExtIndex = dstPath.LastIndexOf(Model3DAttachment.FileExtension);
			if (modelAttachmentExtIndex >= 0) {
				AssetCooker.ModelsToRebuild.Add(dstPath.Remove(modelAttachmentExtIndex) + ".t3d");
			}
			AssetCooker.AssetBundle.ImportFile(srcPath, dstPath, 0, ".txt", AssetAttributes.Zipped, File.GetLastWriteTime(srcPath), AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
