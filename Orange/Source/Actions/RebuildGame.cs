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
			if (The.UI.AskConfirmation("Are you sure you want to rebuild the game?")) {
				CleanupGame();
				AssetCooker.CookForActivePlatform();
				BuildGame();
			}
		}

		static bool CleanupGame()
		{
			string bundlePath = The.Workspace.GetBundlePath();
			var dirInfo = new System.IO.DirectoryInfo(Path.GetDirectoryName(bundlePath));
			foreach (var fileInfo in dirInfo.GetFiles('*' + Path.GetExtension(bundlePath), SearchOption.TopDirectoryOnly)) {
				Console.WriteLine("Deleting {0}", fileInfo.Name);
				File.Delete(fileInfo.FullName);
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
