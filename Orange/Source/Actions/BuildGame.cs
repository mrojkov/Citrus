using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[MenuItem("Build Game")]
		public static void BuildGameAction()
		{
			The.MainWindow.Execute(() => {
				BuildGame();
			});
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
	}
}
