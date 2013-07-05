using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orange
{
	static partial class Actions
	{
		[MenuItem("Rebuild Game")]
		public static void RebuildGameAction()
		{
			if (Toolbox.ShowConfirmationDialog("Are you sure you want to rebuild the game?")) {
				if (CleanupGame()) {
					AssetCooker.CookForActivePlatform();
					BuildGame();
				}
			}
		}

		static bool CleanupGame()
		{
			string bundlePath = The.Workspace.GetBundlePath();
			if (File.Exists(bundlePath)) {
				File.Delete(bundlePath);
			}
			var builder = new SolutionBuilder(The.Workspace.ActivePlatform);
			if (!builder.Clean()) {
				Console.WriteLine("CLEANUP FAILED");
				return false;
			}
			return true;
		}
	}
}
