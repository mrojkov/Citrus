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
			var cookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, The.Workspace.ActiveTarget);
			var bundles = new HashSet<string>();
			foreach (var dictionaryItem in cookingRulesMap) {
				foreach (var bundle in dictionaryItem.Value.Bundles) {
					bundles.Add(bundle);
				}
			}

			foreach (var bundleName in bundles) {
				string bundlePath = The.Workspace.GetBundlePath(bundleName, The.Workspace.ActivePlatform);
				var dirInfo = new System.IO.DirectoryInfo(Path.GetDirectoryName(bundlePath));
				foreach (var fileInfo in dirInfo.GetFiles('*' + Path.GetExtension(bundlePath), SearchOption.TopDirectoryOnly)) {
					UnpackBundle(fileInfo.FullName);
				}
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
							using (var streamCopy = new MemoryStream()) {
								stream.CopyTo(streamCopy);
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
					}
				}
			}
		}

		private static string ChangeExtensionIfKtx(Stream stream, string assetPath)
		{
			if (assetPath.EndsWith(".pvr") && The.Workspace.ActivePlatform == TargetPlatform.Android) {
				using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true)) {
					stream.Seek(0, SeekOrigin.Begin);
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
			using (AssetBundle.Current = new PackedAssetBundle(bundlePath, AssetBundleFlags.None)) {
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
	}
}
