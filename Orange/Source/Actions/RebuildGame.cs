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
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Rebuild Game")]
		[ExportMetadata("Priority", 2)]
		public static void RebuildGameAction()
		{
			if (The.UI.AskConfirmation("Are you sure you want to rebuild the game?")) {
				CleanupGame();
				AssetCooker.CookForActivePlatform();
				BuildGame();
			}
		}

		public static bool CleanupGame()
		{
			string bundlePath = The.Workspace.GetMainBundlePath();
			var dirInfo = new System.IO.DirectoryInfo(Path.GetDirectoryName(bundlePath));
			foreach (var fileInfo in dirInfo.GetFiles('*' + Path.GetExtension(bundlePath), SearchOption.TopDirectoryOnly)) {
				Console.WriteLine("Deleting {0}", fileInfo.Name);
				File.Delete(fileInfo.FullName);
			}
			var builder = new SolutionBuilder(The.Workspace.ActivePlatform, The.Workspace.CustomSolution);
			if (!builder.Clean()) {
				Console.WriteLine("CLEANUP FAILED");
				return false;
			}
			return true;
		}
	}
}
