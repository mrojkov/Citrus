using System;
using System.ComponentModel.Composition;

namespace Orange
{
	public static partial class Actions
	{
		public const string ConsoleCommandPassArguments = "--passargs";

		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Build and Run")]
		[ExportMetadata("Priority", 0)]
		public static string BuildAndRunAction()
		{
			return BuildAndRun(BuildConfiguration.Release);
		}

		public static string BuildAndRun(string configuration)
		{
			AssetCooker.CookForActivePlatform();
			if (!BuildGame(The.Workspace.ActivePlatform, The.Workspace.CustomSolution, configuration)) {
				return "Can not BuildGame";
			}
			The.UI.ScrollLogToEnd();
			RunGame(The.Workspace.ActivePlatform, The.Workspace.CustomSolution, configuration);
			return null;
		}

		public static void RunGame()
		{
			RunGame(
				The.Workspace.ActivePlatform,
				The.Workspace.CustomSolution,
				BuildConfiguration.Release);
		}

		public static bool RunGame(
			TargetPlatform platform, string solutionPath, string configuration)
		{
			var builder = new SolutionBuilder(platform, solutionPath, configuration);
			string arguments = string.Join(" ",
				PluginLoader.GetCommandLineArguments(),
				Toolbox.GetCommandLineArg(ConsoleCommandPassArguments));
			int exitCode = builder.Run(arguments);
			if (exitCode != 0) {
				Console.WriteLine("Application terminated with exit code {0}", exitCode);
				return false;
			}
			return true;
		}
	}
}
