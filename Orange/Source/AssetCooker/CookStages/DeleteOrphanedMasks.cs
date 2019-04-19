using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Orange
{
	class DeleteOrphanedMasks: ICookStage
	{
		public IEnumerable<string> ImportedExtensions { get { yield break; } }
		public IEnumerable<string> BundleExtensions { get { yield return maskExtension; } }

		private readonly string maskExtension = ".mask";

		public void Action()
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
