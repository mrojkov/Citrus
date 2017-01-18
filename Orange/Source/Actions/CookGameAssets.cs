using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class CookGameAssets
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Cook Game Assets")]
		[ExportMetadata("Priority", 4)]
		public static void CookGameAssetsAction()
		{
			AssetCooker.CookForActivePlatform();
		}
	}
}
