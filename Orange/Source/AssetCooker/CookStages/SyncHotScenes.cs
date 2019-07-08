using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncHotScenes : ICookStage
	{
		public IEnumerable<string> ImportedExtensions { get { yield return hotSceneExtension; } }
		public IEnumerable<string> BundleExtensions { get { yield return hotSceneExtension; } }

		private readonly string hotSceneExtension = ".scene";

		public int GetOperationsCount() => SyncUpdated.GetOperationsCount(hotSceneExtension);

		public void Action(Target target) => SyncUpdated.Sync(target, hotSceneExtension, hotSceneExtension, AssetBundle.Current, Converter);

		private bool Converter(Target target, string srcPath, string dstPath)
		{
			using (Stream stream = new FileStream(srcPath, FileMode.Open)) {
				var node = new HotSceneImporter(false, srcPath).Import(stream, null, null);
				Serialization.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, node, Serialization.Format.Binary, hotSceneExtension,
					File.GetLastWriteTime(srcPath), AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
			}
			return true;
		}
	}
}
