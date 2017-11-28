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
			AssetCooker.CookForActivePlatform();
			if (!Actions.BuildGame()) return "Can not BuildGame";
			return null;
		}
	}
}
