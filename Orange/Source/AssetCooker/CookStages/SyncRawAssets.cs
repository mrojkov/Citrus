using System.IO;
using Lime;

namespace Orange
{
	class SyncRawAssets: CookStage
	{
		private string extension;
		private AssetAttributes attributes;

		public SyncRawAssets(string extension, AssetAttributes attributes = AssetAttributes.None)
		{
			SetExtensions(extension);
			this.extension = extension;
			this.attributes = attributes;
		}

		protected override void SetExtensions() { }

		protected void SetExtensions(string extension)
		{
			Extensions = new string[] { extension };
		}

		public override void Action()
		{
			SyncUpdated.Sync(extension, extension, AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			AssetCooker.AssetBundle.ImportFile(srcPath, dstPath, 0, extension, attributes, File.GetLastWriteTime(srcPath), AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
