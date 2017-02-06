#if ORANGE_CLI
using System.ComponentModel.Composition;

namespace Orange
{
	public static class CustomActions
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Build")]
		[ExportMetadata("Priority", 0)]
		public static void BuildAndRunAction()
		{
			AssetCooker.CookForActivePlatform();
			Actions.BuildGame();
		}
	}
}
#endif // ORANGE_CLI
