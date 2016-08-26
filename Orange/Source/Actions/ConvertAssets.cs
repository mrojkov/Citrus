using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class CookGameAssets
	{
		[MenuItem("Cook Game Assets", 4)]
		public static void CookGameAssetsAction()
		{
			AssetCooker.CookForActivePlatform();
		}
	}
}
