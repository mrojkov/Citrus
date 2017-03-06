using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kumquat;
using Lime;

namespace Orange
{
	public static class CodeCooker
	{
		private static readonly string[] scenesExtensions = {".scene", ".tan"};

		public static void Cook(Dictionary<string, CookingRules> cookingRulesMap, IReadOnlyCollection<string> bundles)
		{
			var assetBundle = new AggregateAssetBundle();
			foreach (var b in bundles) {
				assetBundle.Attach(new PackedAssetBundle(The.Workspace.GetBundlePath(b)));
			}
			AssetBundle.Instance = assetBundle;

			The.Workspace.AssetFiles.EnumerationFilter = (info) => scenesExtensions.Contains(Path.GetExtension(info.Path));
			var scenes = The.Workspace.AssetFiles
				.Enumerate()
				.Select(srcFileInfo => srcFileInfo.Path)
				.Where(path => AssetBundle.Instance.FileExists(path))
				.ToDictionary(path => path, path => Node.CreateFromAssetBundle(path));
			The.Workspace.AssetFiles.EnumerationFilter = null;
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
