using System.ComponentModel.Composition;

namespace Orange
{
	public static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Build")]
		[ExportMetadata("Priority", 1)]
		public static string BuildGameAction()
		{
			AssetCooker.CookForActivePlatform();
			return BuildGame() ? null : "Can not BuildGame";
		}

		public static bool BuildGame()
		{
			return BuildGame(The.Workspace.ActivePlatform, The.Workspace.CustomSolution, BuildConfiguration.Release);
		}

		public static bool BuildGame(TargetPlatform platform, string solutionPath, string configuration)
		{
			var builder = new SolutionBuilder(platform, solutionPath, configuration);
			if (The.Workspace.CleanBeforeBuild) {
				builder.Clean();
			}
			if (!builder.Build()) {
				UserInterface.Instance.ExitWithErrorIfPossible();
				return false;
			}
			return true;
		}
	}
}
