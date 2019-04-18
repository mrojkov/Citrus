using System;
using System.IO;
using Lime;
using System.Collections.Generic;
using Orange.FbxImporter;

namespace Orange
{
	class SyncModels : ICookStage
	{
		public IEnumerable<string> ImportedExtensions { get { yield return fbxExtension; } }
		public IEnumerable<string> BundleExtensions { get { yield return t3dExtension; } }

		private readonly string fbxExtension = ".fbx";
		private readonly string t3dExtension = ".t3d";

		public void Action()
		{
			SyncUpdated.Sync(fbxExtension, t3dExtension, AssetBundle.Current, Converter, (srcPath, dstPath) => AssetCooker.ModelsToRebuild.Contains(dstPath));
		}

		private bool Converter(string srcPath, string dstPath)
		{
			var cookingRules = AssetCooker.CookingRulesMap[srcPath];
			var compression = cookingRules.ModelCompression;
			Model3D model;
			var options = new FbxImportOptions {
				Path = srcPath,
				Target = The.Workspace.ActiveTarget,
				CookingRulesMap = AssetCooker.CookingRulesMap
			};
			using (var fbxImporter = new FbxModelImporter(options)) {
				model = fbxImporter.LoadModel();
			}
			AssetAttributes assetAttributes;
			switch (compression) {
				case ModelCompression.None:
					assetAttributes = AssetAttributes.None;
					break;
				case ModelCompression.Deflate:
					assetAttributes = AssetAttributes.ZippedDeflate;
					break;
				case ModelCompression.LZMA:
					assetAttributes = AssetAttributes.ZippedLZMA;
					break;
				default:
					throw new ArgumentOutOfRangeException($"Unknown compression: {compression}");
			}
			var animationPathPrefix = AssetCooker.GetModelAnimationPathPrefix(dstPath);
			AssetCooker.DeleteModelExternalAnimations(animationPathPrefix);
			AssetCooker.ExportModelAnimations(model, animationPathPrefix, assetAttributes, cookingRules.SHA1);
			model.RemoveAnimatorsForExternalAnimations();
			Serialization.WriteObjectToBundle(AssetCooker.AssetBundle, dstPath, model, Serialization.Format.Binary, t3dExtension,
				File.GetLastWriteTime(srcPath), assetAttributes, cookingRules.SHA1);
			return true;
		}
	}
}
