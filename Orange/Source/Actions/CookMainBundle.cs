using System.ComponentModel.Composition;

namespace Orange
{
	static partial class CookMainBundle
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Cook Main Bundle")]
		[ExportMetadata("Priority", 4)]
		public static void CookMainBundleAction()
		{
			var target = The.UI.GetActiveTarget();

			AssetCooker.CookForPlatform(
				target,
				new [] { CookingRulesBuilder.MainBundleName }
			);
		}
	}
}
