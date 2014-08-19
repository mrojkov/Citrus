using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Orange
{
	public static class AssetCooker
	{
		private static readonly List<Action> cookStages = new List<Action>();
		public static IEnumerable<Action> CookStages { get { return cookStages; } }

		private delegate bool Converter(string srcPath, string dstPath);

		private static Lime.AssetsBundle assetsBundle { get { return Lime.AssetsBundle.Instance; } }
		private static TargetPlatform platform;
		private static Dictionary<string, CookingRules> cookingRulesMap;

		const int MaxAtlasChainLength = 1000;

		public static void CookForActivePlatform()
		{
			Cook(The.Workspace.ActivePlatform);
		}

		public static void AddStage(Action action)
		{
			cookStages.Add(action);
		}

		public static void RemoveStage(Action action)
		{
			cookStages.Remove(action);
		}

		static string GetOriginalAssetExtension(string path)
		{
			switch (Path.GetExtension(path)) {
			case ".dds":
			case ".pvr":
			case ".atlasPart":
			case ".mask":
				return ".png";
			case ".sound":
				return ".ogg";
			default:
				return Path.GetExtension(path);
			}
		}

		static string GetPlatformTextureExtension()
		{
			if (platform == TargetPlatform.iOS) {
				return ".pvr";
			} else if (platform == TargetPlatform.Unity) {
				return ".png";
			} else {
				return ".dds";
			}
		}
	
		public static void Cook(TargetPlatform platform)
		{
			AssetCooker.platform = platform;
			cookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles);
			if (platform == TargetPlatform.Unity) {
				CookForUnity();
			} else {
				string bundlePath = The.Workspace.GetBundlePath(platform);
				using (Lime.AssetsBundle.Instance = new Lime.PackedAssetsBundle(bundlePath, Lime.AssetBundleFlags.Writable)) {
					CookHelper();
				}
				// Нужно закрыть бандл, а потом открыть для того чтобы получить достук к 
				// сериализованным сценам (фреймам) для генерации кода - Гриша
				using (Lime.AssetsBundle.Instance = new Lime.PackedAssetsBundle(bundlePath, Lime.AssetBundleFlags.Writable)) {
					using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
						PluginLoader.AfterAssetsCooked();
					}
				}
			}
		}

		private static void CookForUnity()
		{
			string resourcesPath = The.Workspace.GetUnityResourcesDirectory();
			if (!System.IO.Directory.Exists(resourcesPath)) {
				throw new Lime.Exception("Output directory '{0}' doesn't exist", resourcesPath);
			}
			using (Lime.AssetsBundle.Instance = new UnityAssetBundle(resourcesPath)) {
				CookHelper();
			}
		}

		static AssetCooker()
		{
			AddStage(SyncAtlases);
			AddStage(SyncDeleted);
			AddStage(() => SyncRawAssets(".txt"));
			AddStage(SyncTextures);
			AddStage(DeleteOrphanedMasks);
			AddStage(SyncFonts);
			AddStage(() => SyncRawAssets(".ogv"));
			AddStage(SyncScenes);
			AddStage(SyncSounds);
			AddStage(() => SyncRawAssets(".shader"));
			AddStage(() => SyncRawAssets(".xml"));
			AddStage(() => SyncRawAssets(".raw"));
		}

		private static void DeleteOrphanedMasks()
		{
			foreach (var maskPath in assetsBundle.EnumerateFiles()) {
				if (Path.GetExtension(maskPath) == ".mask") {
					var origImageFile = Path.ChangeExtension(maskPath, GetPlatformTextureExtension());
					if (!assetsBundle.FileExists(origImageFile)) {
						Console.WriteLine("- " + maskPath);
						assetsBundle.DeleteFile(maskPath);
					}
				}
			}
		}

		private static void CookHelper()
		{
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				Console.WriteLine("------------- Cooking Assets -------------");
				foreach (var stage in CookStages) {
					stage();
				}
			}
		}

		private static void SyncRawAssets(string extension)
		{
			SyncUpdated(extension, extension, (srcPath, dstPath) => {
				assetsBundle.ImportFile(srcPath, dstPath, 0);
				return true;
			});
		}

		private static void SyncSounds()
		{
			SyncUpdated(".ogg", ".sound", (srcPath, dstPath) => {
				using (var stream = new FileStream(srcPath, FileMode.Open)) {
					// All sounds below 100kb size are converted from OGG to Wav/Adpcm
					if (stream.Length > 100 * 1024) {
						assetsBundle.ImportFile(dstPath, stream, 0);
					} else {
						Console.WriteLine("Converting sound to ADPCM/IMA4 format...");
						using (var input = new Lime.OggDecoder(stream)) {
							using (var output = new MemoryStream()) {
								WaveIMA4Converter.Encode(input, output);
								output.Seek(0, SeekOrigin.Begin);
								assetsBundle.ImportFile(dstPath, output, 0);
							}
						}
					}
					return true;
				}
			});
		}

		private static void SyncScenes()
		{
			SyncUpdated(".scene", ".scene", (srcPath, dstPath) => {
				var importer = HotSceneImporterFactory.CreateImporter(srcPath);
				var node = importer.ParseNode();
				Lime.Serialization.WriteObjectToBundle<Lime.Node>(assetsBundle, dstPath, node);
				return true;
			});
		}

		private static void SyncFonts()
		{
			SyncUpdated(".fnt", ".fnt", (srcPath, dstPath) => {
				string fontPngFile = Path.ChangeExtension(srcPath, ".png");
				Lime.Size size;
				bool hasAlpha;
				if (!TextureConverterUtils.GetPngFileInfo(fontPngFile, out size.Width, out size.Height, out hasAlpha)) {
					throw new Lime.Exception("Font doesn't have an appropriate png texture file");
				}
				var importer = new HotFontImporter(srcPath);
				var font = importer.ParseFont(size);
				for (int i = 0; ; i++) {
					var texturePath = Path.ChangeExtension(dstPath, null);
					string index = (i == 0) ? "" : i.ToString("00");
					string texturePng = Path.ChangeExtension(srcPath, null) + index + ".png";
					if (!File.Exists(texturePng)) {
						break;
					}
					font.Textures.Add(new Lime.SerializableTexture(texturePath + index));
				}
				Lime.Serialization.WriteObjectToBundle<Lime.Font>(assetsBundle, dstPath, font);
				return true;
			});
		}

		private static void SyncTextures()
		{
			SyncUpdated(".png", GetPlatformTextureExtension(), (srcPath, dstPath) => {
				CookingRules rules = cookingRulesMap[Path.ChangeExtension(dstPath, ".png")];
				if (rules.TextureAtlas != null) {
					// No need to cache this texture since it is a part of texture atlas.
					return false;
				}
				if (platform == TargetPlatform.Unity) {
					assetsBundle.ImportFile(srcPath, dstPath, reserve: 0);
				} else {
					string maskPath = Path.ChangeExtension(srcPath, ".mask");
					OpacityMaskCreator.CreateMask(assetsBundle, srcPath, maskPath);
					string tmpFile = Path.ChangeExtension(srcPath, GetPlatformTextureExtension());
					TextureConverter.Convert(srcPath, tmpFile, rules, platform);
                    assetsBundle.ImportFile(tmpFile, dstPath, reserve: 0, compress: true);
					File.Delete(tmpFile);
				}
				return true;
			});
		}

		static void SyncDeleted()
		{
			var assetsFiles = new HashSet<string>();
			foreach (var fileInfo in The.Workspace.AssetFiles.Enumerate()) {
				assetsFiles.Add(fileInfo.Path);
			}
			foreach (string path in assetsBundle.EnumerateFiles()) {
				// Ignoring texture atlases
				if (path.StartsWith("Atlases")) {
					continue;
				}
				// Ignore atlas parts and masks
				var ext = Path.GetExtension(path);
				if (ext == ".atlasPart" || ext == ".mask") {
					continue;
				}
				string assetPath = Path.ChangeExtension(path, GetOriginalAssetExtension(path));
				if (!assetsFiles.Contains(assetPath)) {
					Console.WriteLine("- " + path);
					assetsBundle.DeleteFile(path);
				}
			}
		}

		static void SyncUpdated(string fileExtension, string bundleAssetExtension, Converter converter)
		{
			foreach (var srcFileInfo in The.Workspace.AssetFiles.Enumerate(fileExtension)) {
				string srcPath = srcFileInfo.Path;
				string dstPath = Path.ChangeExtension(srcPath, bundleAssetExtension);
				bool bundled = assetsBundle.FileExists(dstPath);
				bool needUpdate =  !bundled || srcFileInfo.LastWriteTime > assetsBundle.GetFileLastWriteTime(dstPath);
				if (needUpdate) {
					if (converter != null) {
						try {
							if (converter(srcPath, dstPath)) {
								Console.WriteLine((bundled ? "* " : "+ ") + dstPath);
							}
						} catch (System.Exception) {
							Console.WriteLine("An exception was caught while processing '{0}'\n", srcPath);
							throw;
						}
					} else {
						Console.WriteLine((bundled ? "* " : "+ ") + dstPath);
						using (Stream stream = new FileStream(srcPath, FileMode.Open, FileAccess.Read)) {
							assetsBundle.ImportFile(dstPath, stream, 0);
						}
					}
				}
			}
		}
	
		class AtlasItem
		{
			public string Path;
			public Gdk.Pixbuf Pixbuf;
			public Lime.IntRectangle AtlasRect;
			public bool Allocated;
			public bool MipMapped;
			public PVRFormat PVRFormat;
			public DDSFormat DDSFormat;
		}

		static string GetAtlasPath(string atlasChain, int index)
		{
			var path = Lime.AssetPath.Combine("Atlases", atlasChain + "." + index.ToString("000") + GetPlatformTextureExtension());
			return path;
		}

		static void BuildAtlasChain(string atlasChain)
		{
			for (int i = 0; i < MaxAtlasChainLength; i++) {
				string atlasPath = GetAtlasPath(atlasChain, i);
				if (assetsBundle.FileExists(atlasPath)) {
					Console.WriteLine("- " + atlasPath);
					assetsBundle.DeleteFile(atlasPath);
				} else {
					break;
				}
			}
			var maxAtlasSize = GetMaxAtlasSize();
			var items = new List<AtlasItem>();
			foreach (var p in cookingRulesMap) {
				if (p.Value.TextureAtlas == atlasChain && Path.GetExtension(p.Key) == ".png") {
					var srcTexturePath = Lime.AssetPath.Combine(The.Workspace.AssetsDirectory, p.Key);
					var pixbuf = new Gdk.Pixbuf(srcTexturePath);
					// Ensure that no image exceede maxAtlasSize limit
					if (pixbuf.Width > maxAtlasSize.Width || pixbuf.Height > maxAtlasSize.Height) {
						int w = Math.Min(pixbuf.Width, maxAtlasSize.Width);
						int h = Math.Min(pixbuf.Height, maxAtlasSize.Height);
						pixbuf = pixbuf.ScaleSimple(w, h, Gdk.InterpType.Bilinear);
						Console.WriteLine(
							String.Format("WARNING: {0} downscaled to {1}x{2}", srcTexturePath, w, h));
					}
					var item = new AtlasItem {
						Path = Path.ChangeExtension(p.Key, ".atlasPart"), 
						Pixbuf = pixbuf,
						MipMapped = p.Value.MipMaps,
						PVRFormat = p.Value.PVRFormat,
						DDSFormat = p.Value.DDSFormat
					};
					items.Add(item);
				}
			}
			// Sort images in descendend size order
			items.Sort((x, y) => {
				int a = Math.Max(x.Pixbuf.Width, x.Pixbuf.Height);
				int b = Math.Max(y.Pixbuf.Width, y.Pixbuf.Height);
				return b - a;
			});	
			// PVRTC4 textures must be square
			var squareAtlas = (platform == TargetPlatform.iOS) && items.Max(i => i.PVRFormat) == PVRFormat.PVRTC4;
			for (int atlasId = 0; items.Count > 0; atlasId++) {
				if (atlasId >= MaxAtlasChainLength) {
					throw new Lime.Exception("Too many textures in the atlas chain {0}", atlasChain);
				}
				var bestSize = new Lime.Size(0, 0);
				double bestPackRate = 0;
				foreach (var size in EnumerateAtlasSizes(squareAtlas: squareAtlas)) {
					double packRate;
					PackItemsToAtlas(items, size, out packRate);
					if (packRate * 0.95f > bestPackRate) {
						bestPackRate = packRate;
						bestSize = size;
					}
				}
				if (bestPackRate == 0) {
					throw new Lime.Exception("Failed to create atlas '{0}'", atlasChain);
				}
				PackItemsToAtlas(items, bestSize, out bestPackRate);
				CopyAllocatedItemsToAtlas(items, atlasChain, atlasId, bestSize);
				items.RemoveAll(x => x.Allocated);
			}
		}

		private static IEnumerable<Lime.Size> EnumerateAtlasSizes(bool squareAtlas)
		{
			if (squareAtlas) {
				for (int i = 64; i <= GetMaxAtlasSize().Width; i *= 2) {
					yield return new Lime.Size(i, i);
				}
			} else {
				for (int i = 64; i <= GetMaxAtlasSize().Width / 2; i *= 2) {
					yield return new Lime.Size(i, i);
					yield return new Lime.Size(i * 2, i);
					yield return new Lime.Size(i, i * 2);
				}
				yield return GetMaxAtlasSize();
			}
		}

		private static Lime.Size GetMaxAtlasSize()
		{
			return (platform == TargetPlatform.Desktop) ? new Lime.Size(2048, 2048) : new Lime.Size(1024, 1024);
		}

		private static void PackItemsToAtlas(List<AtlasItem> items, Lime.Size size, out double packRate)
		{
			items.ForEach(i => i.Allocated = false);
			// Take in account 1 pixel border for each side.
			var a = new RectAllocator(new Lime.Size(size.Width + 2, size.Height + 2));
			foreach (var item in items) {
				var sz = new Lime.Size(item.Pixbuf.Width + 2, item.Pixbuf.Height + 2);
				if (a.Allocate(sz, out item.AtlasRect)) {
					item.Allocated = true;
				}
			}
			packRate = a.GetPackRate();
		}

		private static void CopyAllocatedItemsToAtlas(List<AtlasItem> items, string atlasChain, int atlasId, Lime.Size size)
		{
			string atlasPath = GetAtlasPath(atlasChain, atlasId);
			var atlas = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, size.Width, size.Height);
			atlas.Fill(0);
			var rules = new CookingRules() {
				MipMaps = false,
				PVRFormat = PVRFormat.PVRTC4,
				DDSFormat = DDSFormat.DXTi
			};
			foreach (var item in items.Where(i => i.Allocated)) {
				if (item.PVRFormat > rules.PVRFormat)
					rules.PVRFormat = item.PVRFormat;
				if (item.DDSFormat > rules.DDSFormat)
					rules.DDSFormat = item.DDSFormat;
				rules.MipMaps |= item.MipMapped;
				var p = item.Pixbuf;
				p.CopyArea(0, 0, p.Width, p.Height, atlas, item.AtlasRect.A.X, item.AtlasRect.A.Y);
				var atlasPart = new Lime.TextureAtlasPart();
				atlasPart.AtlasRect = item.AtlasRect;
				atlasPart.AtlasRect.B -= new Lime.IntVector2(2, 2);
				atlasPart.AtlasTexture = Path.ChangeExtension(atlasPath, null);

				//Console.WriteLine("+ " + item.Path);
				Lime.Serialization.WriteObjectToBundle<Lime.TextureAtlasPart>(assetsBundle, item.Path, atlasPart);

				// Delete non-atlased texture since now its useless
				var texturePath = Path.ChangeExtension(item.Path, GetPlatformTextureExtension());
				if (assetsBundle.FileExists(texturePath)) {
					Console.WriteLine("- " + texturePath);
					assetsBundle.DeleteFile(texturePath);
				}
			}
			Console.WriteLine("+ " + atlasPath);
			if (platform == TargetPlatform.Unity) {
				throw new NotImplementedException();
				// assetsBundle.ImportFile(inFile, atlasPath, 0);
			} else {
				var tmpFile = GetTempFilePathWithExtension(GetPlatformTextureExtension());
				string maskPath = Path.ChangeExtension(atlasPath, ".mask");
				OpacityMaskCreator.CreateMask(assetsBundle, atlas, maskPath);
				TextureConverter.Convert(atlas, tmpFile, rules, platform);
				assetsBundle.ImportFile(tmpFile, atlasPath, 0, compress: true);
				File.Delete(tmpFile);
			}
		}

		static string GetTempFilePathWithExtension(string extension)
		{
			var path = Path.GetTempPath();
			var fileName = Guid.NewGuid().ToString() + extension;
			return Path.Combine(path, fileName);
		}

		static void SyncAtlases()
		{
			var textures = new Dictionary<string, DateTime>();
			foreach (var fileInfo in The.Workspace.AssetFiles.Enumerate(".png")) {
				textures[fileInfo.Path] = fileInfo.LastWriteTime;
			}
			var atlasChainsToRebuild = new HashSet<string>();
			// Figure out atlas chains to rebuld
			foreach (string atlasPartPath in assetsBundle.EnumerateFiles()) {
				if (Path.GetExtension(atlasPartPath) != ".atlasPart")
					continue;
				// If atlas part has been outdated we should rebuild full atlas chain
				string srcTexturePath =  Path.ChangeExtension(atlasPartPath, ".png");
				if (!textures.ContainsKey(srcTexturePath) || assetsBundle.GetFileLastWriteTime(atlasPartPath) < textures[srcTexturePath]) {
					srcTexturePath = Lime.AssetPath.Combine(The.Workspace.AssetsDirectory, srcTexturePath);
					var part = Lime.TextureAtlasPart.ReadFromBundle(atlasPartPath);
					string atlasChain = Path.GetFileNameWithoutExtension(part.AtlasTexture);
					atlasChainsToRebuild.Add(atlasChain);
					if (!File.Exists(srcTexturePath)) {
						Console.WriteLine("- " + atlasPartPath);
						assetsBundle.DeleteFile(atlasPartPath);
					} else {
						srcTexturePath = Path.ChangeExtension(atlasPartPath, ".png");
						if (cookingRulesMap[srcTexturePath].TextureAtlas != null) {
							CookingRules rules = cookingRulesMap[srcTexturePath];
							atlasChainsToRebuild.Add(rules.TextureAtlas);
						} else {
							Console.WriteLine("- " + atlasPartPath);
							assetsBundle.DeleteFile(atlasPartPath);
						}
					}
				}
			}
			// Find which new textures must be added to the atlas chain
			foreach (var p in cookingRulesMap) {
				string atlasPartPath = Path.ChangeExtension(p.Key, ".atlasPart");
				bool atlasNeedRebuld = p.Value.TextureAtlas != null && 
					Path.GetExtension(p.Key) == ".png" && !assetsBundle.FileExists(atlasPartPath);
				if (atlasNeedRebuld) {
					atlasChainsToRebuild.Add(p.Value.TextureAtlas);
				}
			}
			foreach (string atlasChain in atlasChainsToRebuild) {
				BuildAtlasChain(atlasChain);
			}
		}
	}
}

