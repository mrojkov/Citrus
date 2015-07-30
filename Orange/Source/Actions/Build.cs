using System;

namespace Orange
{
	public static partial class Actions
	{
		[MenuItem("Build", 1)]
		public static void BuildAction()
		{
			AssetCooker.CookForActivePlatform();
			BuildGame();
		}
	}
}
