using System.ComponentModel.Composition;

namespace Orange
{
	static class DeleteUnpackedBundles
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Delete Unpacked Bundles")]
		[ExportMetadata("Priority", 31)]
		[ExportMetadata("ApplicableToBundleSubset", true)]
		public static void DeleteUnpackedBundlesAction() => AssetsUnpacker.Delete(The.UI.GetActiveTarget(), The.UI.GetSelectedBundles());
	}
}
