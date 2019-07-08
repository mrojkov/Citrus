using System.ComponentModel.Composition;

namespace Orange
{
	public static class CustomActions
	{
		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Build")]
		[ExportMetadata("Priority", 0)]
		public static string BuildAndRunAction()
		{
			var target = The.UI.GetActiveTarget();

			AssetCooker.CookForPlatform(target);
			if (!Actions.BuildGame(target)) {
				return "Can not BuildGame";
			}
			return null;
		}
	}
}
