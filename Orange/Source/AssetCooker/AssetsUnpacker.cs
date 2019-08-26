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
		private const string UnpackedSuffix = ".Unpacked";

		public static void Unpack(TargetPlatform platform)
		{
			Unpack(platform, AssetCooker.GetListOfAllBundles());
		}

		public static void Unpack(TargetPlatform platform, List<string> bundles)
		{
			The.UI.SetupProgressBar(GetAssetsToRevealCount(bundles));
			foreach (var bundleName in bundles) {
				string bundlePath = The.Workspace.GetBundlePath(bundleName, platform);
				UnpackBundle(bundlePath);
			}
			The.UI.StopProgressBar();
		}

		public static void Delete(TargetPlatform platform)
		{
			var bundles = AssetCooker.GetListOfAllBundles();
			The.UI.SetupProgressBar(bundles.Count);
			foreach (var bundleName in bundles) {
				string bundlePath = The.Workspace.GetBundlePath(bundleName, platform) + UnpackedSuffix;
				DeleteBundle(bundlePath);
				The.UI.IncreaseProgressBar();
			}
			The.UI.StopProgressBar();
		}

		private static void UnpackBundle(string bundlePath)
		{
			if (!File.Exists(bundlePath)) {
				Console.WriteLine($"WARNING: {bundlePath} do not exists! Skipping...");
				return;
			}
			string outputDirectory = bundlePath + UnpackedSuffix;
			using (var bundle = new PackedAssetBundle(bundlePath, AssetBundleFlags.None)) {
				AssetBundle.SetCurrent(bundle, false);
				Console.WriteLine("Extracting game content into \"{0}\"", outputDirectory);
				if (Directory.Exists(outputDirectory)) {
					Directory.Delete(outputDirectory, true);
				}
				Directory.CreateDirectory(outputDirectory);
				using (new DirectoryChanger(outputDirectory)) {
					foreach (string asset in AssetBundle.Current.EnumerateFiles()) {
						using (var stream = AssetBundle.Current.OpenFile(asset)) {
							using (var streamCopy = new MemoryStream()) {
								stream.CopyTo(streamCopy);
								streamCopy.Seek(0, SeekOrigin.Begin);
								var assetPath = ChangeExtensionIfKtx(streamCopy, asset);
								Console.WriteLine("> " + assetPath);
								var assetDirectory = Path.GetDirectoryName(assetPath);
								if (assetDirectory != "") {
									Directory.CreateDirectory(assetDirectory);
								}
								using (var file = new FileStream(assetPath, FileMode.Create)) {
									streamCopy.CopyTo(file);
								}
							}
						}
						The.UI.IncreaseProgressBar();
					}
				}
			}
		}

		private static string ChangeExtensionIfKtx(Stream stream, string assetPath)
		{
			if (assetPath.EndsWith(".pvr") && The.Workspace.ActivePlatform == TargetPlatform.Android) {
				using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true)) {
					var sign = reader.ReadInt32();
					stream.Seek(0, SeekOrigin.Begin);
					if (sign == Texture2D.KTXMagic) {
						assetPath = Path.ChangeExtension(assetPath, ".ktx");
					}
				}
			}
			return assetPath;
		}

		public static void UnpackTangerineScenes()
		{
			string bundlePath = The.Workspace.GetMainBundlePath();
			string outputDirectory = The.Workspace.AssetsDirectory;
			using (var bundle = new PackedAssetBundle(bundlePath, AssetBundleFlags.None)) {
				AssetBundle.SetCurrent(bundle, false);
				Console.WriteLine("Extracting tangerine scenes into \"{0}\"", outputDirectory);
				using (new DirectoryChanger(outputDirectory)) {
					foreach (string asset in AssetBundle.Current.EnumerateFiles()) {
						if (asset.EndsWith(".scene", StringComparison.OrdinalIgnoreCase)) {
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

		private static int GetAssetsToRevealCount(List<string> bundles)
		{
			var assetCount = 0;
			foreach (var bundleName in bundles) {
				string bundlePath = The.Workspace.GetBundlePath(bundleName, The.Workspace.ActivePlatform);
				if (!File.Exists(bundlePath)) {
					continue;
				}
				using (var bundle = new PackedAssetBundle(bundlePath, AssetBundleFlags.None)) {
						AssetBundle.SetCurrent(bundle, false);
						assetCount += AssetBundle.Current.EnumerateFiles().Count();
				}
			}
			return assetCount;
		}

		public static void DeleteBundle(string bundlePath)
		{
			if (!Directory.Exists(bundlePath)) {
				Console.WriteLine($"WARNING: {bundlePath} do not exists! Skipping...");
				return;
			}
			try {
				Directory.Delete(bundlePath, true);
				Console.WriteLine($"{bundlePath} deleted.");
			} catch (System.Exception exception) {
				Console.WriteLine($"{bundlePath} deletion skipped because of exception: {exception.Message}");
			}
		}
	}
}
