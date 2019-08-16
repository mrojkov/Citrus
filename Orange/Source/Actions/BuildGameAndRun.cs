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
			var target = The.UI.GetActiveTarget();
			return BuildAndRun(target, BuildConfiguration.Release);
		}

		public static string BuildAndRun(Target target, string configuration)
		{
			AssetCooker.CookForTarget(target);
			if (!BuildGame(target, configuration)) {
				return "Can not BuildGame";
			}
			The.UI.ScrollLogToEnd();
			RunGame(target.Platform, target.ProjectPath, configuration);
			return null;
		}

		public static void RunGame(Target target)
		{
			RunGame(
				target.Platform,
				target.ProjectPath,
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
