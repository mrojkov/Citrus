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
			AssetCooker.Cook(
				The.Workspace.ActivePlatform,
				new System.Collections.Generic.List<string>() { CookingRulesBuilder.MainBundleName }
			);
		}
	}
}
