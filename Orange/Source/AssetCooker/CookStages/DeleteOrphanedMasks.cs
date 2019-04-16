using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Orange
{
	class DeleteOrphanedMasks: CookStage
	{
		public override IEnumerable<string> ImportedExtensions { get { yield break; } }
		public override IEnumerable<string> BundleExtensions { get { yield return maskExtension; } }

		private readonly string maskExtension = ".mask";

		public override void Action()
		{
			foreach (var maskPath in AssetCooker.AssetBundle.EnumerateFiles().ToList()) {
				if (maskPath.EndsWith(maskExtension, StringComparison.OrdinalIgnoreCase)) {
					var origImageFile = Path.ChangeExtension(maskPath, AssetCooker.GetPlatformTextureExtension());
					if (!AssetCooker.AssetBundle.FileExists(origImageFile)) {
						AssetCooker.DeleteFileFromBundle(maskPath);
					}
				}
			}
		}
	}
}
