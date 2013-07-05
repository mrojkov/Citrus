using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[MenuItem("Reveal Cooked Assets")]
		public static void RevealCookedAssetsAction()
		{
			AssetsUnpacker.Unpack(The.Workspace.ActivePlatform);
		}
	}
}
