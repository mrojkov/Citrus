using System;
using System.ComponentModel.Composition;
using System.IO;

namespace Orange
{
	public static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Build & Run")]
		[ExportMetadata("Priority", 0)]
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
			return BuildGame(The.Workspace.ActivePlatform, The.Workspace.CustomSolution);
		}

		public static bool BuildGame(TargetPlatform platform, string customSolution = null)
		{
			var builder = new SolutionBuilder(platform, customSolution);
			if (The.Workspace.CleanBeforeBuild) {
				builder.Clean();
			}

			if (!builder.Build()) {
				Console.WriteLine("BUILD FAILED");
				if (UserInterface.Instance is ConsoleUI) {
					Environment.Exit(1);
				}
				return false;
			}
			return true;
		}

		public static void RunGame()
		{
			RunGame(The.Workspace.ActivePlatform, The.Workspace.CustomSolution);
		}

		public static bool RunGame(TargetPlatform platform, string customSolution = null)
		{
			var builder = new SolutionBuilder(platform, customSolution);
			string arguments = PluginLoader.GetCommandLineArguments();
			int exitCode = builder.Run(arguments);
			if (exitCode != 0) {
				Console.WriteLine("Application terminated with exit code {0}", exitCode);
				return false;
			}
			return true;
		}
	}
}
