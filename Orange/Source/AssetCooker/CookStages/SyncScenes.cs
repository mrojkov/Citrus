using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncScenes : AssetCookerCookStage, ICookStage
	{
		public IEnumerable<string> ImportedExtensions { get { yield return sceneExtension; } }
		public IEnumerable<string> BundleExtensions { get { yield return sceneExtension; } }

		private readonly string sceneExtension = ".tan";

		public SyncScenes(AssetCooker assetCooker) : base(assetCooker) { }

		public int GetOperationsCount() => SyncUpdated.GetOperationsCount(sceneExtension);

		public void Action() => SyncUpdated.Sync(sceneExtension, sceneExtension, AssetBundle.Current, Converter);

		private bool Converter(string srcPath, string dstPath)
		{
			var node = InternalPersistence.Instance.ReadObjectFromFile<Node>(srcPath);
			InternalPersistence.Instance.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, node, Persistence.Format.Binary, sceneExtension,
				File.GetLastWriteTime(srcPath), AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
