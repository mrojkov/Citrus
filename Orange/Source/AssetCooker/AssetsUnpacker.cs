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
		public static void Unpack(TargetPlatform platform)
		{
			string bundlePath = The.Workspace.GetMainBundlePath(platform);
			var dirInfo = new System.IO.DirectoryInfo(Path.GetDirectoryName(bundlePath));
			foreach (var fileInfo in dirInfo.GetFiles('*' + Path.GetExtension(bundlePath), SearchOption.TopDirectoryOnly)) {
				UnpackBundle(fileInfo.FullName);
			}
		}

		private static void UnpackBundle(string bundlePath)
		{
			string outputDirectory = bundlePath + ".Unpacked";
			using (AssetBundle.Current = new PackedAssetBundle(bundlePath, AssetBundleFlags.None)) {
				Console.WriteLine("Extracting game content into \"{0}\"", outputDirectory);
				if (Directory.Exists(outputDirectory)) {
					Directory.Delete(outputDirectory, true);
				}
				Directory.CreateDirectory(outputDirectory);
				using (new DirectoryChanger(outputDirectory)) {
					foreach (string asset in AssetBundle.Current.EnumerateFiles()) {
						using (var stream = AssetBundle.Current.OpenFile(asset)) {
							Console.WriteLine("> " + asset);
							string assetDirectory = Path.GetDirectoryName(asset);
							if (assetDirectory != "") {
								Directory.CreateDirectory(assetDirectory);
							}
							using (var file = new FileStream(asset, FileMode.Create)) {
								stream.CopyTo(file);
							}
						}
					}
				}
			}
		}

		public static void UnpackTangerineScenes()
		{
			string bundlePath = The.Workspace.GetMainBundlePath();
			string outputDirectory = The.Workspace.AssetsDirectory;
			using (AssetBundle.Current = new PackedAssetBundle(bundlePath, AssetBundleFlags.None)) {
				Console.WriteLine("Extracting tangerine scenes into \"{0}\"", outputDirectory);
				using (new DirectoryChanger(outputDirectory)) {
					foreach (string asset in AssetBundle.Current.EnumerateFiles()) {
						if (Path.GetExtension(asset) == ".scene") {
							using (var stream = AssetBundle.Current.OpenFile(asset)) {
								var	outputPath = Path.ChangeExtension(asset, ".tan");
								Console.WriteLine("> " + outputPath);
								var buffer = new byte[stream.Length];
								stream.Read(buffer, 0, buffer.Length);
								File.WriteAllBytes(outputPath, buffer);
							}
						}
					}
				}
			}
		}
	}
}
