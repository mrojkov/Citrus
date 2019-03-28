using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Lime;

namespace Orange
{
	class SyncDeleted: CookStage
	{
		public SyncDeleted(): base()
		{

		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".atlasPart", ".mask", ".texture", ".ant", ".t3d" };
			ImportedExtension = null;
			ExportedExtension = null;
		}

		public override void Action()
		{
			var assetFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var fileInfo in The.Workspace.AssetFiles.Enumerate()) {
				assetFiles.Add(fileInfo.Path);
			}
			foreach (var path in AssetCooker.AssetBundle.EnumerateFiles().ToList()) {
				// Ignoring texture atlases
				if (path.StartsWith("Atlases")) {
					continue;
				}
				// Ignore atlas parts, masks, animations
				var ext = Path.GetExtension(path);
				if (Extensions.ToList().Contains(ext, StringComparer.OrdinalIgnoreCase) && ext != Extensions.Last()) {
					continue;
				}
				var assetPath = Path.ChangeExtension(path, AssetCooker.GetOriginalAssetExtension(path));
				if (!assetFiles.Contains(assetPath)) {
					if (path.EndsWith(Extensions.Last(), StringComparison.OrdinalIgnoreCase)) {
						AssetCooker.DeleteModelExternalAnimations(AssetCooker.GetModelAnimationPathPrefix(path));
					}
					var modelAttachmentExtIndex = path.LastIndexOf(Model3DAttachment.FileExtension);
					if (modelAttachmentExtIndex >= 0) {
						AssetCooker.ModelsToRebuild.Add(path.Remove(modelAttachmentExtIndex) + Extensions.Last());
					}
					AssetCooker.DeleteFileFromBundle(path);
				}
			}
		}
	}
}
