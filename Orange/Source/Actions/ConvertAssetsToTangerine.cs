using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lime;

namespace Orange
{
	static partial class ConvertAssets
	{
		[MenuItem("Convert Assets to Tangerine Format", 50)]
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
					if (path.EndsWith(".scene")) {
						var importer = HotSceneImporterFactory.CreateImporter(srcPath);
						var node = importer.ParseNode();
						dstPath = Path.ChangeExtension(dstPath, "tan");
						Serialization.WriteObjectToFile(dstPath, node, Serialization.Format.JSON);
					} else if (path.EndsWith(".fnt")) {
						dstPath = Path.ChangeExtension(dstPath, "tft");
						var importer = new HotFontImporter();
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
