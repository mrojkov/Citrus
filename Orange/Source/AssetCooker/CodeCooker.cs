using System.Collections.Generic;
using System.Linq;
using Kumquat;
using Lime;
using Orange;

namespace Orange
{
	public static class CodeCooker
	{
		public static void Cook(Dictionary<string, CookingRules> cookingRulesMap, IReadOnlyCollection<string> bundles)
		{
			var assetBundle = new AggregateAssetBundle();
			foreach (var b in bundles) {
				assetBundle.Attach(new PackedAssetBundle(The.Workspace.GetBundlePath(b)));
			}
			AssetBundle.Instance = assetBundle;
			var scenes = The.Workspace.AssetFiles
				.Enumerate(".scene")
				.Select(srcFileInfo => srcFileInfo.Path)
				.Where(path => AssetBundle.Instance.FileExists(path))
				.ToDictionary(path => path, path => new Frame(path));
			if (scenes.Count == 0) {
				return;
			}

			var sceneToBundleMap = cookingRulesMap.ToDictionary(i => i.Key, i => i.Value.Bundles[0]);
			new ScenesCodeCooker(The.Workspace.ProjectDirectory, The.Workspace.Title, CookingRules.MainBundleName,
				sceneToBundleMap, scenes).Start();
			AssetBundle.Instance = null;
		}
	}
}
