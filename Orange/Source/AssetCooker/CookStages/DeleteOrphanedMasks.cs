using System;
using System.Linq;
using System.IO;

namespace Orange
{
	class DeleteOrphanedMasks: CookStage
	{
		public DeleteOrphanedMasks(): base()
		{

		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".mask" };
		}

		public override void Action()
		{
			foreach (var maskPath in AssetCooker.AssetBundle.EnumerateFiles().ToList()) {
				if (maskPath.EndsWith(".mask", StringComparison.OrdinalIgnoreCase)) {
					var origImageFile = Path.ChangeExtension(maskPath, AssetCooker.GetPlatformTextureExtension());
					if (!AssetCooker.AssetBundle.FileExists(origImageFile)) {
						AssetCooker.DeleteFileFromBundle(maskPath);
					}
				}
			}
		}
	}
}
