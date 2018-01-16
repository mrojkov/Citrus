using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kumquat;
using Lime;

namespace Orange
{
	public static class CodeCooker
	{
		private static readonly string[] scenesExtensions = { ".scene", ".tan", ".model" };

		public static void Cook(IReadOnlyCollection<string> bundles)
		{
			var assetBundles = new Dictionary<string, PackedAssetBundle>(bundles.Count);
			foreach (var bundleName in bundles) {
				assetBundles.Add(bundleName, new PackedAssetBundle(The.Workspace.GetBundlePath(bundleName)));
			}

			try {
				AssetBundle.Current = new AggregateAssetBundle(assetBundles.Values.Cast<AssetBundle>().ToArray());

				Func<string, bool> filter = (path) => scenesExtensions.Contains(Path.GetExtension(path));
				var scenes = new Dictionary<string, Node>();
				var sceneToBundleMap = new Dictionary<string, string>();
				foreach (var bundle in assetBundles) {
					var bundleScenesFiles = bundle.Value
						.EnumerateFiles()
						.Where(filter)
						.ToList();

					foreach (var sceneFile in bundleScenesFiles) {
						scenes.Add(sceneFile, Node.CreateFromAssetBundle(sceneFile));
						sceneToBundleMap.Add(sceneFile, bundle.Key);
					}
				}

				if (scenes.Count == 0) {
					return;
				}
				new ScenesCodeCooker(
					The.Workspace.ProjectDirectory,
					The.Workspace.Title,
					CookingRulesBuilder.MainBundleName,
					sceneToBundleMap,
					scenes
				).Start();
			} finally {
				AssetBundle.Current.Dispose();
				AssetBundle.Current = null;
			}
		}
	}
}
