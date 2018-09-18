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
		public static void RevealCookedAssetsAction()
		{
			AssetsUnpacker.Unpack(The.Workspace.ActivePlatform);
		}
	}
}
