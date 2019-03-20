using System;
using System.Linq;
using System.IO;

namespace Orange
{
	class DeleteOrphanedTextureParams: CookStage
	{
		public DeleteOrphanedTextureParams(): base()
		{

		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".texture" };
		}

		public override void Action()
		{
			foreach (var path in AssetCooker.AssetBundle.EnumerateFiles().ToList()) {
				if (path.EndsWith(".texture", StringComparison.OrdinalIgnoreCase)) {
					var origImageFile = Path.ChangeExtension(path, AssetCooker.GetPlatformTextureExtension());
					if (!AssetCooker.AssetBundle.FileExists(origImageFile)) {
						AssetCooker.DeleteFileFromBundle(path);
					}
				}
			}
		}
	}
}
