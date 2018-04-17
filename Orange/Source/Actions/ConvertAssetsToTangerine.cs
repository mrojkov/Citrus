using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Lime;

namespace Orange
{
	static partial class ConvertAssets
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Convert Assets to Tangerine Format")]
		[ExportMetadata("Priority", 50)]
		public static void ConvertAssetsAction()
		{
			var fileEnumerator = The.Workspace.AssetFiles;
			var tanDataRoot = Path.Combine(The.Workspace.ProjectDirectory, The.Workspace.AssetsDirectory + "_Tangerine\\");
			DirectoryInfo di = Directory.CreateDirectory(tanDataRoot);
			using (new DirectoryChanger(fileEnumerator.Directory)) {
				foreach (var fileInfo in fileEnumerator.Enumerate()) {
					var path = fileInfo.Path;
					var dstPath = Path.Combine(tanDataRoot, path);
					var dstDir = Path.GetDirectoryName(dstPath);
					var srcPath = path;
					Directory.CreateDirectory(dstDir);
					if (path.EndsWith(".scene", StringComparison.OrdinalIgnoreCase)) {
						using (Stream stream = new FileStream(srcPath, FileMode.Open)) {
							var node = new HotSceneImporter(false, srcPath).Import(stream, null, null);
							dstPath = Path.ChangeExtension(dstPath, "tan");
							Serialization.WriteObjectToFile(dstPath, node, Serialization.Format.JSON);
						}
					} else if (path.EndsWith(".fnt", StringComparison.OrdinalIgnoreCase)) {
						dstPath = Path.ChangeExtension(dstPath, "tft");
						var importer = new HotFontImporter(false);
						var dstRelativePath = Path.ChangeExtension(srcPath, "tft");
						var font = importer.ParseFont(srcPath, dstRelativePath);
						Serialization.WriteObjectToFile(dstPath, font, Serialization.Format.JSON);
					} else {
						File.Copy(srcPath, dstPath, true);
					}
				}
			}
		}
	}
}
