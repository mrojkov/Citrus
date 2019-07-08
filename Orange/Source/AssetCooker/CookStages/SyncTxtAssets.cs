using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncTxtAssets: ICookStage
	{
		public IEnumerable<string> ImportedExtensions { get { yield return txtExtension; } }
		public IEnumerable<string> BundleExtensions { get { yield return txtExtension; } }

		private readonly string txtExtension = ".txt";
		private readonly string t3dExtension = ".t3d";

		public int GetOperationsCount() => SyncUpdated.GetOperationsCount(txtExtension);

		public void Action(Target target) => SyncUpdated.Sync(target, txtExtension, txtExtension, AssetBundle.Current, Converter);

		private bool Converter(Target target, string srcPath, string dstPath)
		{
			var modelAttachmentExtIndex = dstPath.LastIndexOf(Model3DAttachment.FileExtension);
			if (modelAttachmentExtIndex >= 0) {
				AssetCooker.ModelsToRebuild.Add(dstPath.Remove(modelAttachmentExtIndex) + t3dExtension);
			}
			AssetCooker.AssetBundle.ImportFile(srcPath, dstPath, 0, txtExtension, AssetAttributes.Zipped, File.GetLastWriteTime(srcPath), AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
