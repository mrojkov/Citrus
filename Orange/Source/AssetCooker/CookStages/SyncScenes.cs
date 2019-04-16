using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncScenes : CookStage
	{
		public override IEnumerable<string> ImportedExtensions { get { yield return sceneExtension; } }
		public override IEnumerable<string> BundleExtensions { get { yield return sceneExtension; } }

		private readonly string sceneExtension = ".tan";

		public override void Action()
		{
			SyncUpdated.Sync(sceneExtension, sceneExtension, AssetBundle.Current, Converter);
		}

		private bool Converter(string srcPath, string dstPath)
		{
			var node = Serialization.ReadObjectFromFile<Node>(srcPath);
			Serialization.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, node, Serialization.Format.Binary, sceneExtension,
				File.GetLastWriteTime(srcPath), AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
