using System;
using System.IO;
using System.Collections.Generic;

namespace Orange
{
	public class AssetCooker
	{
		private delegate bool Converter(string srcPath, string dstPath);

		private Lime.AssetsBundle assetsBundle { get { return Lime.AssetsBundle.Instance; } }
		private CitrusProject project;
		private TargetPlatform platform;
		private Dictionary<string, CookingRules> cookingRulesMap;

		string GetOriginalAssetExtension(string path)
		{
			switch (Path.GetExtension(path)) {
			case ".dds":
			case ".pvr":
			case ".atlasPart":
				return ".png";
			case ".sound":
				return ".ogg";
			default:
				return Path.GetExtension(path);
			}
		}

		string GetPlatformTextureExtension()
		{
			if (platform == TargetPlatform.iOS)
				return ".pvr";
			else
				return ".dds";
		}

		public AssetCooker(CitrusProject project, TargetPlatform platform)
		{
			this.platform = platform;
			this.project = project;
		}
		
		public void Cook()
		{
			cookingRulesMap = CookingRulesBuilder.Build(project.AssetFiles);
			string bundlePath = Path.ChangeExtension(project.AssetsDirectory, Helpers.GetTargetPlatformString(platform));
			using (Lime.AssetsBundle.Instance = new Lime.PackedAssetsBundle(bundlePath, Lime.AssetBundleFlags.Writable)) {
				using (new DirectoryChanger(project.AssetsDirectory)) {
					Console.WriteLine("------------- Building Game Content -------------");
					SyncAtlases();
					SyncDeleted();
					SyncUpdated(".txt", ".txt", (srcPath, dstPath) => {
						assetsBundle.ImportFile(srcPath, dstPath, 0);
						return true;
					});
					SyncUpdated(".png", GetPlatformTextureExtension(), (srcPath, dstPath) => {
						CookingRules rules = cookingRulesMap[Path.ChangeExtension(dstPath, ".png")];
						if (rules.TextureAtlas != null) {
							// No need to cache this texture since it is a part of texture atlas.
							return false;
						}
						string tmpFile = Path.ChangeExtension(srcPath, GetPlatformTextureExtension());
						TextureConverter.Convert(srcPath, tmpFile, rules, platform);
						assetsBundle.ImportFile(tmpFile, dstPath, reserve: 0, compress: true);
						File.Delete(tmpFile);
						return true;
					});
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
							string texturePng = Path.Combine(Path.ChangeExtension(srcPath, null) + index + ".png");
							if (!File.Exists(Path.Combine(texturePng))) {
								break;
							}
							font.Textures.Add(new Lime.SerializableTexture(texturePath + index));
						}
						Lime.Serialization.WriteObjectToBundle<Lime.Font>(assetsBundle, dstPath, font);
						return true;
					});
					SyncUpdated(".scene", ".scene", (srcPath, dstPath) => {
						var importer = new HotSceneImporter(srcPath);
						var node = importer.ParseNode();
						Lime.Serialization.WriteObjectToBundle<Lime.Node>(assetsBundle, dstPath, node);
						return true;
					});
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
					SyncUpdated(".ogv", ".ogv", (srcPath, dstPath) => {
						assetsBundle.ImportFile(srcPath, dstPath, 0);
						return true;
					});
				}
			}
		}
	
		void SyncDeleted()
		{
			var assetsFiles = new HashSet<string>();
			foreach (var fileInfo in project.AssetFiles.Enumerate()) {
				assetsFiles.Add(fileInfo.Path);
			}
			foreach (string path in assetsBundle.EnumerateFiles()) {
				// Ignoring texture atlases
				if (path.StartsWith("Atlases")) {
					continue;
				}
				// Ignore atlas parts
				if (Path.GetExtension(path) == ".atlasPart") {
					continue;
				}
				string assetPath = Path.ChangeExtension(path, GetOriginalAssetExtension(path));
				if (!assetsFiles.Contains(assetPath)) {
					Console.WriteLine("- " + path);
					assetsBundle.DeleteFile(path);
				}
			}
		}

		void SyncUpdated(string fileExtension, string bundleAssetExtension, Converter converter)
		{
			foreach (var srcFileInfo in project.AssetFiles.Enumerate(fileExtension)) {
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
		
		string GetAtlasPath(string atlasChain, int index)
		{
			var path = Lime.AssetPath.Combine("Atlases", atlasChain + "." + index.ToString("00") + GetPlatformTextureExtension());
			return path;
		}
		
		void BuildAtlasChain(string atlasChain)
		{
			for (int i = 0; i < 100; i++) {
				string atlasPath = GetAtlasPath(atlasChain, i);
				if (assetsBundle.FileExists(atlasPath)) {
					Console.WriteLine("- " + atlasPath);
					assetsBundle.DeleteFile(atlasPath);
				} else {
					break;
				}
			}
			int maxAtlasSize = (platform == TargetPlatform.Desktop) ? 2048 : 1024;
			var items = new List<AtlasItem>();
			foreach (var p in cookingRulesMap) {
				if (p.Value.TextureAtlas == atlasChain && Path.GetExtension(p.Key) == ".png") {
					var srcTexturePath = Lime.AssetPath.Combine(project.AssetsDirectory, p.Key);
					var pixbuf = new Gdk.Pixbuf(srcTexturePath);
					// Ensure that no image exceede maxAtlasSize limit
					if (pixbuf.Width > maxAtlasSize || pixbuf.Height > maxAtlasSize) {
						int w = Math.Min(pixbuf.Width, maxAtlasSize);
						int h = Math.Min(pixbuf.Height, maxAtlasSize);
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
			for (int atlasId = 0; items.Count > 0; atlasId++) {
				for (int i = 64; i <= maxAtlasSize; i *= 2) {
					foreach (AtlasItem item in items) {
						item.Allocated = false;
					}
					// Take in account 1 pixel border for each side.
					var a = new RectAllocator(new Lime.Size(i + 2, i + 2));
					bool allAllocated = true;
					foreach (AtlasItem item in items) {
						var size = new Lime.Size(item.Pixbuf.Width + 2, item.Pixbuf.Height + 2);
						if (a.Allocate(size, out item.AtlasRect)) {
							item.Allocated = true;
						} else {
							allAllocated = false;
						}
					}
					if (i != maxAtlasSize && !allAllocated) {
						continue;
					}
					if (atlasId > 99) {
						throw new Lime.Exception("Too many textures in the atlas chain {0}", atlasChain);
					}
					string atlasPath = GetAtlasPath(atlasChain, atlasId);
					var atlas = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, i, i);
					atlas.Fill(0);
					var rules = new CookingRules() { 
						MipMaps = false, 
						PVRFormat = PVRFormat.PVRTC4,
						DDSFormat = DDSFormat.DXTi
					};
					foreach (AtlasItem item in items) {
						if (!item.Allocated) {
							continue;
						}
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
					string inFile = "$TMP$.png";
					string outFile = Path.ChangeExtension(inFile, GetPlatformTextureExtension());
					if (!atlas.Save(inFile, "png")) {
						var error = (Gdk.PixbufError)Gdk.Pixbuf.ErrorQuark();
						throw new Lime.Exception("Can't save '{0}' (error: {1})", inFile, error.ToString()); 
					}
					TextureConverter.Convert(inFile, outFile, rules, platform);
					assetsBundle.ImportFile(outFile, atlasPath, 0, true);
					File.Delete(inFile);
					File.Delete(outFile);
					items.RemoveAll(x => x.Allocated);
					break;
				}
			}
		}
		
		void SyncAtlases()
		{
			var textures = new Dictionary<string, DateTime>();
			foreach (var fileInfo in project.AssetFiles.Enumerate(".png")) {
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
					srcTexturePath = Lime.AssetPath.Combine(project.AssetsDirectory, srcTexturePath);
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

