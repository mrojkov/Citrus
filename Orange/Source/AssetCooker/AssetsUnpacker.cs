using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;
using System.IO;

namespace Orange
{
	public static class AssetsUnpacker
	{
		public static void Unpack(CitrusProject project, TargetPlatform platform)
		{
			string bundlePath = Path.ChangeExtension(project.AssetsDirectory, Helpers.GetTargetPlatformString(platform));
			string outputDirectory = bundlePath + ".Unpacked";
			AssetsBundle.Instance.Open(bundlePath, AssetBundleFlags.None);
			try {
				Console.WriteLine("Extracting game content into \"{0}\"", outputDirectory);
				if (Directory.Exists(outputDirectory)) {
					Directory.Delete(outputDirectory, true);
				}
				Directory.CreateDirectory(outputDirectory);
				using (new DirectoryChanger(outputDirectory)) {
					foreach (string asset in AssetsBundle.Instance.EnumerateFiles()) {
						using (var stream = AssetsBundle.Instance.OpenFile(asset)) {
							Console.WriteLine("> " + asset);
							string assetDirectory = Path.GetDirectoryName(asset);
							if (assetDirectory != "") {
								Directory.CreateDirectory(assetDirectory);
							}
							var buffer = new byte[stream.Length];
							stream.Read(buffer, 0, buffer.Length);
							File.WriteAllBytes(asset, buffer);
						}
					}
				}
			} finally {
				AssetsBundle.Instance.Close();
			}
		}

		public static void UnpackTangerineScenes(CitrusProject project, TargetPlatform platform)
		{
			string bundlePath = Path.ChangeExtension(project.AssetsDirectory, Helpers.GetTargetPlatformString(platform));
			string outputDirectory = project.AssetsDirectory;
			AssetsBundle.Instance.Open(bundlePath, AssetBundleFlags.None);
			try {
				Console.WriteLine("Extracting tangerine scenes into \"{0}\"", outputDirectory);
				using (new DirectoryChanger(outputDirectory)) {
					foreach (string asset in AssetsBundle.Instance.EnumerateFiles()) {
						if (Path.GetExtension(asset) == ".scene") {
							using (var stream = AssetsBundle.Instance.OpenFile(asset)) {
								var	outputPath = Path.ChangeExtension(asset, ".tan");
								Console.WriteLine("> " + outputPath);
								var buffer = new byte[stream.Length];
								stream.Read(buffer, 0, buffer.Length);
								File.WriteAllBytes(outputPath, buffer);
							}
						}
					}
				}
			} finally {
				AssetsBundle.Instance.Close();
			}		
		}
	}
}
