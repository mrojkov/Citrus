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
			if (The.UI.AskConfirmation("Are you sure you want to rebuild the game?")) {
				CleanupGame();
				AssetCooker.CookForActivePlatform();
				if (!BuildGame()) return "Can not BuildGame";
			}
			return null;
		}

		public static bool CleanupGame()
		{
			DeleteAllBundlesInTopDirectory();
			DeleteAllBundlesReferredInCookingRules();

			var builder = new SolutionBuilder(The.Workspace.ActivePlatform, The.Workspace.CustomSolution);
			if (!builder.Clean()) {
				Console.WriteLine("CLEANUP FAILED");
				return false;
			}
			return true;
		}

		private static void DeleteAllBundlesReferredInCookingRules()
		{
			var bundles = GetAllBundles();
			foreach (var path in bundles.Select(bundle => The.Workspace.GetBundlePath(bundle)).Where(File.Exists)) {
				try {
					Console.WriteLine("Deleting {0}", path);
					File.Delete(path);
				} catch (System.Exception e) {
					Console.WriteLine("Can not remove {0}: {1}", path, e.Message);
				}
			}
		}

		private static void DeleteAllBundlesInTopDirectory()
		{
			string bundlePath = The.Workspace.GetMainBundlePath();
			var dirInfo = new System.IO.DirectoryInfo(Path.GetDirectoryName(bundlePath));
			foreach (var fileInfo in dirInfo.GetFiles('*' + Path.GetExtension(bundlePath), SearchOption.TopDirectoryOnly)) {
				Console.WriteLine("Deleting {0}", fileInfo.Name);
				File.Delete(fileInfo.FullName);
			}
		}

		private static HashSet<string> GetAllBundles()
		{
			var bundles = new HashSet<string>() {
				CookingRulesBuilder.MainBundleName
			};
			var cookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, The.Workspace.ActiveTarget);
			foreach (var bundle in cookingRulesMap.SelectMany(i => i.Value.Bundles.Where(bundle => bundle != CookingRulesBuilder.MainBundleName))) {
				bundles.Add(bundle);
			}
			return bundles;
		}
	}
}
