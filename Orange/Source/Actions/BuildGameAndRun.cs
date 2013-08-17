using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[MenuItem("Build & Run", 0)]
		public static void BuildAndRunAction()
		{
			AssetCooker.CookForActivePlatform();
			if (BuildGame()) {
				The.UI.ScrollLogToEnd();
				RunGame();
			}
		}

		public static bool BuildGame()
		{
			return BuildGame(The.Workspace.ActivePlatform);
		}

		public static bool BuildGame(TargetPlatform platform)
		{
			var builder = new SolutionBuilder(platform);
			if (!builder.Build()) {
				Console.WriteLine("BUILD FAILED");
				return false;
			}
			return true;
		}

		public static void RunGame()
		{
			RunGame(The.Workspace.ActivePlatform);
		}

		public static bool RunGame(TargetPlatform platform)
		{
			var builder = new SolutionBuilder(platform);
			int exitCode = builder.Run("");
			if (exitCode != 0) {
				Console.WriteLine("Application terminated with exit code {0}", exitCode);
				return false;
			}
			return true;
		}
	}
}
