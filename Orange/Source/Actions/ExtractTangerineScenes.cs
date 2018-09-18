using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		// [MenuItem("Extract Tangerine Scenes")]
		public static void ExtractTangerineScenes()
		{
			AssetsUnpacker.UnpackTangerineScenes();
		}
	}
}
