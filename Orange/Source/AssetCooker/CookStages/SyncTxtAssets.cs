using System.IO;
using System.Linq;
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
			ImportedExtension = Extensions[0];
			ExportedExtension = Extensions[0];
		}

		public override void Action()
		{
			SyncUpdated.Sync(ImportedExtension, ExportedExtension, AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			var modelAttachmentExtIndex = dstPath.LastIndexOf(Model3DAttachment.FileExtension);
			if (modelAttachmentExtIndex >= 0) {
				AssetCooker.ModelsToRebuild.Add(dstPath.Remove(modelAttachmentExtIndex) + Extensions.Last());
			}
			AssetCooker.AssetBundle.ImportFile(srcPath, dstPath, 0, ImportedExtension, AssetAttributes.Zipped, File.GetLastWriteTime(srcPath), AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
