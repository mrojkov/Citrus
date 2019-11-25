using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	class SyncCompoundFonts : AssetCookerCookStage, ICookStage
	{
		public IEnumerable<string> ImportedExtensions { get { yield return fontExtension; } }
		public IEnumerable<string> BundleExtensions { get { yield return fontExtension; } }

		private readonly string fontExtension = ".cft";

		public SyncCompoundFonts(AssetCooker assetCooker) : base(assetCooker) { }

		public int GetOperationsCount() => SyncUpdated.GetOperationsCount(fontExtension);

		public void Action() => SyncUpdated.Sync(fontExtension, fontExtension, AssetBundle.Current, Converter);

		private bool Converter(string srcPath, string dstPath)
		{
			var font = InternalPersistence.Instance.ReadObjectFromFile<SerializableCompoundFont>(srcPath);
			InternalPersistence.Instance.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, font, Persistence.Format.Binary, fontExtension,
				File.GetLastWriteTime(srcPath), AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
			return true;
		}
	}
}
