using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;

namespace Orange
{
	class RemoveDeprecatedModels : AssetCookerCookStage, ICookStage
	{
		public IEnumerable<string> ImportedExtensions { get { yield break; } }
		public IEnumerable<string> BundleExtensions { get { yield return modelExtension; } }

		private readonly string modelExtension = ".model";

		public RemoveDeprecatedModels(AssetCooker assetCooker) : base(assetCooker) { }

		public int GetOperationsCount() => The.Workspace.AssetFiles.Enumerate(modelExtension).Count();

		public void Action()
		{
			foreach (var fileInfo in The.Workspace.AssetFiles.Enumerate(modelExtension)) {
				var path = fileInfo.Path;
				if (AssetCooker.CookingRulesMap.ContainsKey(path)) {
					AssetCooker.CookingRulesMap.Remove(path);
				}
				Logger.Write($"Removing deprecated .model file: {path}");
				File.Delete(path);
				UserInterface.Instance.IncreaseProgressBar();
			}
		}
	}
}
