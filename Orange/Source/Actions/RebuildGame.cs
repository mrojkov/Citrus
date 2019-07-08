using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.IO;

namespace Orange
{
	static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Rebuild Game")]
		[ExportMetadata("Priority", 2)]
		public static string RebuildGameAction()
		{
			var target = The.UI.GetActiveTarget();

			if (The.UI.AskConfirmation("Are you sure you want to rebuild the game?")) {
				CleanupGame(target);
				AssetCooker.CookForPlatform(target);
				if (!BuildGame(target)) {
					return "Can not BuildGame";
				}
			}
			return null;
		}

		public static bool CleanupGame(Target target)
		{
			DeleteAllBundlesInTopDirectory(target);
			DeleteAllBundlesReferredInCookingRules(target);

			var builder = new SolutionBuilder(target.Platform, target.ProjectPath);
			if (!builder.Clean()) {
				Console.WriteLine("CLEANUP FAILED");
				return false;
			}
			return true;
		}

		private static void DeleteAllBundlesReferredInCookingRules(Target target)
		{
			var bundles = GetAllBundles(target);
			foreach (var path in bundles.Select(bundle => The.Workspace.GetBundlePath(target.Platform, bundle)).Where(File.Exists)) {
				try {
					Console.WriteLine("Deleting {0}", path);
					File.Delete(path);
				} catch (System.Exception e) {
					Console.WriteLine("Can not remove {0}: {1}", path, e.Message);
				}
			}
		}

		private static void DeleteAllBundlesInTopDirectory(Target target)
		{
			string bundlePath = The.Workspace.GetMainBundlePath(target.Platform);
			var dirInfo = new System.IO.DirectoryInfo(Path.GetDirectoryName(bundlePath));
			foreach (var fileInfo in dirInfo.GetFiles('*' + Path.GetExtension(bundlePath), SearchOption.TopDirectoryOnly)) {
				Console.WriteLine("Deleting {0}", fileInfo.Name);
				File.Delete(fileInfo.FullName);
			}
		}

		private static HashSet<string> GetAllBundles(Target target)
		{
			var bundles = new HashSet<string>() {
				CookingRulesBuilder.MainBundleName
			};
			var cookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, target);
			foreach (var bundle in cookingRulesMap.SelectMany(i => i.Value.Bundles.Where(bundle => bundle != CookingRulesBuilder.MainBundleName))) {
				bundles.Add(bundle);
			}
			return bundles;
		}
	}
}
