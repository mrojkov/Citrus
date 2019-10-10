using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Reveal Cooked Assets")]
		[ExportMetadata("Priority", 30)]
		[ExportMetadata("ApplicableToBundleSubset", true)]
		public static void RevealCookedAssetsAction()
		{
			var target = The.UI.GetActiveTarget();

			AssetsUnpacker.Unpack(target, The.UI.GetSelectedBundles());
		}
	}
}
