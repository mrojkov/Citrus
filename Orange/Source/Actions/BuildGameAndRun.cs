using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[MenuItem("Build Game & Run", 0)]
		public static void BuildGameAndRunAction()
		{
			The.MainWindow.Execute(() => {
				AssetCooker.BuildForActivePlatform();
				if (BuildGame()) {
					The.MainWindow.ScrollLogToEnd();
					RunGame();
				}
			});
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
