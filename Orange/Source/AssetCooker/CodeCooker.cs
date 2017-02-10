using System.Collections.Generic;
using System.Linq;
using Kumquat;
using Lime;
using Orange;

namespace Orange
{
	public static class CodeCooker
	{
		public static void Cook()
		{
			var scenes = The.Workspace.AssetFiles
				.Enumerate(".scene")
				.Select(srcFileInfo => srcFileInfo.Path)
				.Where(path => AssetsBundle.Instance.FileExists(path))
				.ToDictionary(path => path, path => new Frame(path));
			if (scenes.Count <= 0) {
				return;
			}

			var sceneToBundleMap = CookingRulesBuilder.Build(
				The.Workspace.AssetFiles,
				The.Workspace.ActivePlatform,
				The.Workspace.Target
			).ToDictionary(i => i.Key, i => i.Value.Bundles[0]);
			new ScenesCodeCooker(The.Workspace.ProjectDirectory, The.Workspace.Title, sceneToBundleMap, scenes).Start();
		}
	}
}
