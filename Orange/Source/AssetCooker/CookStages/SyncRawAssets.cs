using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncRawAssets: CookStage
	{
		public override IEnumerable<string> ImportedExtensions { get { yield return extension; } }
		public override IEnumerable<string> BundleExtensions { get { yield return extension; } }

		private readonly string extension;
		private readonly AssetAttributes attributes;

		public SyncRawAssets(string extension, AssetAttributes attributes = AssetAttributes.None)
		{
			this.extension = extension;
			this.attributes = attributes;
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
