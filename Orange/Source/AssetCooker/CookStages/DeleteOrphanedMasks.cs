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
			ImportedExtension = null;
			ExportedExtension = null;
		}

		public override void Action()
		{
			foreach (var maskPath in AssetCooker.AssetBundle.EnumerateFiles().ToList()) {
				if (maskPath.EndsWith(Extensions[0], StringComparison.OrdinalIgnoreCase)) {
					var origImageFile = Path.ChangeExtension(maskPath, AssetCooker.GetPlatformTextureExtension());
					if (!AssetCooker.AssetBundle.FileExists(origImageFile)) {
						AssetCooker.DeleteFileFromBundle(maskPath);
					}
				}
			}
		}
	}
}
