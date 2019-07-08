using System.ComponentModel.Composition;

namespace Orange
{
	static partial class CookGameAssets
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Cook Game Assets")]
		[ExportMetadata("Priority", 4)]
		public static void CookGameAssetsAction()
		{
			var target = The.UI.GetActiveTarget();

			AssetCooker.CookForPlatform(target);
		}
	}
}
