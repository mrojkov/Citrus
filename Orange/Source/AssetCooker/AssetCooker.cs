using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Lime;

namespace Orange
{
	public enum CookingProfile
	{
		Total,
		Partial
	}

	public static class AssetCooker
	{
		private static readonly CookingProfile[] defaultCookingProfiles = { CookingProfile.Total, CookingProfile.Partial };
		private static readonly Dictionary<Action, CookingProfile[]> cookStages = new Dictionary<Action, CookingProfile[]>();
		private static CookingProfile cookingProfile = CookingProfile.Total;
		public static IEnumerable<Action> CookStages => cookStages.Keys;

		private delegate bool Converter(string srcPath, string dstPath);

		public static AssetBundle AssetBundle => AssetBundle.Current;
		public static TargetPlatform Platform;
		private static Dictionary<string, CookingRules> cookingRulesMap;

		private static string atlasesPostfix = string.Empty;

		public const int MaxAtlasChainLength = 1000;

		public static event Action BeginCookBundles;
		public static event Action EndCookBundles;

		private static bool cookCanceled = false;
		private static ICollection<string> bundleBackupFiles;

		public static void CookForActivePlatform()
		{
			var skipCooking = The.Workspace.ProjectJson.GetValue<bool>("SkipAssetsCooking");
			if (!skipCooking) {
				Cook(The.Workspace.ActivePlatform);
			}
			else {
				Console.WriteLine("-------------  Skip Assets Cooking -------------");
			}
		}

		public static void AddStage(Action action, params CookingProfile[] cookingProfiles)
		{
			cookStages.Add(action, cookingProfiles.Length == 0 ? defaultCookingProfiles : cookingProfiles);
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
			case ".jpg":
				return ".png";
			case ".sound":
				return ".ogg";
			case ".model":
				return ".fbx";
			default:
				return Path.GetExtension(path);
			}
		}

		public static string GetPlatformTextureExtension()
		{
			switch (Platform) {
				case TargetPlatform.iOS:
				case TargetPlatform.Android:
					return ".pvr";
				case TargetPlatform.Unity:
					return ".png";
				default:
					return ".dds";
			}
		}

		public static void Cook(TargetPlatform platform, List<string> bundles = null)
		{
			AssetCooker.Platform = platform;
			cookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, The.Workspace.ActiveTarget);
			CookBundles(bundles: bundles);
		}

		public static void CookCustomAssets(TargetPlatform platform, List<string> assets)
		{
			AssetCooker.Platform = platform;
			cookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, The.Workspace.ActiveTarget);

			var defaultAssetsEnumerator = The.Workspace.AssetFiles;
			var assetsFileInfo = assets
				.Select(asset => new FileInfo { Path = asset, LastWriteTime = DateTime.Now })
				.ToList();
			The.Workspace.AssetFiles = new CustomFilesEnumerator(defaultAssetsEnumerator.Directory, assetsFileInfo);

			var defaultCookingProfile = AssetCooker.cookingProfile;
			AssetCooker.cookingProfile = CookingProfile.Partial;

			CookBundles(requiredCookCode: false);
			The.Workspace.AssetFiles = defaultAssetsEnumerator;
			AssetCooker.cookingProfile = defaultCookingProfile;
		}

		private static void CookBundles(bool requiredCookCode = true, List<string> bundles = null)
		{
			var extraBundles = new HashSet<string>();
			foreach (var dictionaryItem in cookingRulesMap) {
				foreach (var bundle in dictionaryItem.Value.Bundles) {
					if (bundle != CookingRulesBuilder.MainBundleName && (bundles == null || bundles.Contains(bundle))) {
						extraBundles.Add(bundle);
					}
				}
			}

			try {
				BeginCookBundles?.Invoke();

				CookBundle(CookingRulesBuilder.MainBundleName);
				foreach (var extraBundle in extraBundles) {
					CookBundle(extraBundle);
				}
				extraBundles.Add(CookingRulesBuilder.MainBundleName);

				var extraBundlesList = extraBundles.ToList();
				PluginLoader.AfterBundlesCooked(extraBundlesList);
				if (requiredCookCode) {
					CodeCooker.Cook(extraBundlesList);
				}
			} catch (OperationCanceledException e) {
				Console.WriteLine(e.Message);
				RestoreBackups();
			} finally {
				cookCanceled = false;
				RemoveBackups();
				EndCookBundles?.Invoke();
			}
		}

		private static void CookBundle(string bundleName)
		{
			string bundlePath = The.Workspace.GetBundlePath(bundleName);
			string backupFilePath;
			TryMakeBackup(bundlePath, out backupFilePath);
			bundleBackupFiles.Add(backupFilePath);

			using (AssetBundle.Current = CreateBundle(bundleName)) {
				CookBundleHelper(bundleName);
			}
			// Open the bundle again in order to make some plugin post-processing (e.g. generate code from scene assets)
			using (AssetBundle.Current = CreateBundle(bundleName)) {
				using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
					PluginLoader.AfterAssetsCooked(bundleName);
				}
			}
			if (Platform != TargetPlatform.Unity) {
				PackedAssetBundle.RefreshBundleCheckSum(bundlePath);
			}
		}

		private static AssetBundle CreateBundle(string bundleName)
		{
			if (Platform == TargetPlatform.Unity) {
				var path = The.Workspace.GetUnityProjectDirectory();
				path = bundleName == CookingRulesBuilder.MainBundleName ?
					Path.Combine(path, "Assets", "Resources") :
					Path.Combine(path, "Assets", "Bundles", bundleName);
				Directory.CreateDirectory(path);
				return new UnityAssetBundle(path);
			}
			var bundlePath = The.Workspace.GetBundlePath(bundleName);

			// Create directory for bundle if it placed in subdirectory
			Directory.CreateDirectory(Path.GetDirectoryName(bundlePath));

			return new PackedAssetBundle(bundlePath, AssetBundleFlags.Writable);
		}

		private static void CookBundleHelper(string bundleName)
		{
			Console.WriteLine("------------- Cooking Assets ({0}) -------------", bundleName);
			The.Workspace.AssetFiles.EnumerationFilter = (info) => {
				CookingRules rules;
				if (cookingRulesMap.TryGetValue(info.Path, out rules)) {
					if (rules.Ignore)
						return false;
					return Array.IndexOf(rules.Bundles, bundleName) != -1;
				} else {
					// There are no cooking rules for text files, consider them as part of the main bundle.
					return bundleName == CookingRulesBuilder.MainBundleName;
				}
			};
			// Every asset bundle must have its own atlases folder, so they aren't conflict with each other
			atlasesPostfix = bundleName != CookingRulesBuilder.MainBundleName ? bundleName : "";
			try {
				using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
					var profileCookStages = cookStages
						.Where(kv => kv.Value.Contains(cookingProfile))
						.Select(kv => kv.Key);
					foreach (var stage in profileCookStages) {
						CheckCookCancelation();
						stage();
					}
				}
			} finally {
				The.Workspace.AssetFiles.EnumerationFilter = null;
				atlasesPostfix = "";
			}
		}

		static AssetCooker()
		{
			bundleBackupFiles = new List<String>();

			AddStage(SyncModels);
			AddStage(SyncAtlases, CookingProfile.Total);
			AddStage(SyncDeleted, CookingProfile.Total);
			AddStage(() => SyncRawAssets(".json", AssetAttributes.ZippedDeflate));
			AddStage(() => SyncRawAssets(".txt", AssetAttributes.ZippedDeflate));
			AddStage(() => SyncRawAssets(".csv", AssetAttributes.ZippedDeflate));
			var rawAssetExtensions = The.Workspace.ProjectJson["RawAssetExtensions"] as string;
			if (rawAssetExtensions != null) {
				foreach (var extension in rawAssetExtensions.Split(' ')) {
					AddStage(() => SyncRawAssets(extension, AssetAttributes.ZippedDeflate));
				}
			}
			AddStage(SyncTextures, CookingProfile.Total);
			AddStage(DeleteOrphanedMasks, CookingProfile.Total);
			AddStage(DeleteOrphanedTextureParams, CookingProfile.Total);
			AddStage(SyncFonts);
			AddStage(SyncHotFonts);
			AddStage(() => SyncRawAssets(".ttf"));
			AddStage(() => SyncRawAssets(".otf"));
			AddStage(() => SyncRawAssets(".ogv"));
			AddStage(SyncScenes);
			AddStage(SyncHotScenes);
			AddStage(SyncSounds);
			AddStage(() => SyncRawAssets(".shader"));
			AddStage(() => SyncRawAssets(".xml"));
			AddStage(() => SyncRawAssets(".raw"));
			AddStage(WarnAboutNPOTTextures, CookingProfile.Total);
			AddStage(() => SyncRawAssets(".bin"));
		}

		private static void WarnAboutNPOTTextures()
		{
			foreach (var path in AssetBundle.EnumerateFiles()) {
				if ((AssetBundle.GetAttributes(path) & AssetAttributes.NonPowerOf2Texture) != 0) {
					Console.WriteLine("Warning: non-power of two texture: {0}", path);
				}
			}
		}

		public static void DeleteFileFromBundle(string path)
		{
			Console.WriteLine("- " + path);
			AssetBundle.DeleteFile(path);
		}

		private static void DeleteOrphanedTextureParams()
		{
			foreach (var path in AssetBundle.EnumerateFiles()) {
				if (Path.GetExtension(path) == ".texture") {
					var origImageFile = Path.ChangeExtension(path, GetPlatformTextureExtension());
					if (!AssetBundle.FileExists(origImageFile)) {
						DeleteFileFromBundle(path);
					}
				}
			}
		}

		private static void DeleteOrphanedMasks()
		{
			foreach (var maskPath in AssetBundle.EnumerateFiles()) {
				if (Path.GetExtension(maskPath) == ".mask") {
					var origImageFile = Path.ChangeExtension(maskPath, GetPlatformTextureExtension());
					if (!AssetBundle.FileExists(origImageFile)) {
						DeleteFileFromBundle(maskPath);
					}
				}
			}
		}

		private static void SyncRawAssets(string extension, AssetAttributes attributes = AssetAttributes.None)
		{
			SyncUpdated(extension, extension, (srcPath, dstPath) => {
				AssetBundle.ImportFile(srcPath, dstPath, 0, extension, attributes, cookingRulesMap[srcPath].SHA1);
				return true;
			});
		}

		private static void SyncSounds()
		{
			const string sourceExtension = ".ogg";
			if (Platform == TargetPlatform.Unity) {
				SyncRawAssets(sourceExtension);
				return;
			}
			SyncUpdated(sourceExtension, ".sound", (srcPath, dstPath) => {
				using (var stream = new FileStream(srcPath, FileMode.Open)) {
					// All sounds below 100kb size (can be changed with cooking rules) are converted
					// from OGG to Wav/Adpcm
					var rules = cookingRulesMap[srcPath];
					if (stream.Length > rules.ADPCMLimit * 1024) {
						AssetBundle.ImportFile(dstPath, stream, 0, sourceExtension, AssetAttributes.None, cookingRulesMap[srcPath].SHA1);
					} else {
						Console.WriteLine("Converting sound to ADPCM/IMA4 format...");
						using (var input = new OggDecoder(stream)) {
							using (var output = new MemoryStream()) {
								WaveIMA4Converter.Encode(input, output);
								output.Seek(0, SeekOrigin.Begin);
								AssetBundle.ImportFile(dstPath, output, 0, sourceExtension, AssetAttributes.None, cookingRulesMap[srcPath].SHA1);
							}
						}
					}
					return true;
				}
			});
		}

		private static void SyncScenes()
		{
			SyncUpdated(".tan", ".tan", (srcPath, dstPath) => {
				var node = Serialization.ReadObjectFromFile<Node>(srcPath);
				Serialization.WriteObjectToBundle(AssetBundle, dstPath, node, Serialization.Format.Binary, ".tan", AssetAttributes.None, cookingRulesMap[srcPath].SHA1);
				return true;
			});
		}

		private static void SyncFonts()
		{
			SyncUpdated(".tft", ".tft", (srcPath, dstPath) => {
				var font = Serialization.ReadObjectFromFile<Font>(srcPath);
				Serialization.WriteObjectToBundle(AssetBundle, dstPath, font, Serialization.Format.Binary, ".tft", AssetAttributes.None, cookingRulesMap[srcPath].SHA1);
				return true;
			});
		}

		private static void SyncHotScenes()
		{
			SyncUpdated(".scene", ".scene", (srcPath, dstPath) => {
				using (Stream stream = new FileStream(srcPath, FileMode.Open)) {
					var node = new HotSceneImporter(false, srcPath).Import(stream, null, null);
					Serialization.WriteObjectToBundle(AssetBundle, dstPath, node, Serialization.Format.Binary, ".scene", AssetAttributes.None, cookingRulesMap[srcPath].SHA1);
				}
				return true;
			});
		}

		private static void SyncHotFonts()
		{
			SyncUpdated(".fnt", ".fnt", (srcPath, dstPath) => {
				var importer = new HotFontImporter(false);
				var font = importer.ParseFont(srcPath, dstPath);
				Serialization.WriteObjectToBundle(AssetBundle, dstPath, font, Serialization.Format.Binary, ".fnt", AssetAttributes.None, cookingRulesMap[srcPath].SHA1);
				return true;
			});
		}

		private static void SyncTextures()
		{
			SyncUpdated(".png", GetPlatformTextureExtension(), (srcPath, dstPath) => {
				var rules = cookingRulesMap[Path.ChangeExtension(dstPath, ".png")];
				if (rules.TextureAtlas != null) {
					// No need to cache this texture since it is a part of texture atlas.
					return false;
				}
				if (Platform == TargetPlatform.Unity) {
					AssetBundle.ImportFile(srcPath, dstPath, 0, ".png", AssetAttributes.None, cookingRulesMap[srcPath].SHA1);
				}
				else {
					using (var stream = File.OpenRead(srcPath)) {
						var bitmap = new Bitmap(stream);
						if (ShouldDownscale(bitmap, rules)) {
							var scaledBitmap = DownscaleTexture(bitmap, srcPath, rules);
							bitmap.Dispose();
							bitmap = scaledBitmap;
						}
						ImportTexture(dstPath, bitmap, rules, rules.SHA1);
						bitmap.Dispose();
					}
				}
				return true;
			});
		}

		static void SyncDeleted()
		{
			var assetFiles = new HashSet<string>();
			foreach (var fileInfo in The.Workspace.AssetFiles.Enumerate()) {
				assetFiles.Add(fileInfo.Path);
			}
			foreach (var path in AssetBundle.EnumerateFiles()) {
				// Ignoring texture atlases
				if (path.StartsWith("Atlases")) {
					continue;
				}
				// Ignore atlas parts and masks
				var ext = Path.GetExtension(path);
				if (ext == ".atlasPart" || ext == ".mask" || ext == ".texture") {
					continue;
				}
				var assetPath = Path.ChangeExtension(path, GetOriginalAssetExtension(path));
				if (!assetFiles.Contains(assetPath)) {
					DeleteFileFromBundle(path);
				}
			}
		}

		static void SyncUpdated(string fileExtension, string bundleAssetExtension, Converter converter)
		{
			SyncUpdated(fileExtension, bundleAssetExtension, AssetBundle.Current, converter);
		}

		static void SyncUpdated(string fileExtension, string bundleAssetExtension, AssetBundle bundle, Converter converter)
		{
			foreach (var srcFileInfo in The.Workspace.AssetFiles.Enumerate(fileExtension)) {
				var srcPath = srcFileInfo.Path;
				var dstPath = Path.ChangeExtension(srcPath, bundleAssetExtension);
				var bundled = bundle.FileExists(dstPath);
				var srcRules = cookingRulesMap[srcPath];
				var needUpdate = !bundled || srcFileInfo.LastWriteTime > bundle.GetFileLastWriteTime(dstPath);
				needUpdate = needUpdate || !srcRules.SHA1.SequenceEqual(bundle.GetCookingRulesSHA1(dstPath));
				if (needUpdate) {
					if (converter != null) {
						try {
							if (converter(srcPath, dstPath)) {
								Console.WriteLine((bundled ? "* " : "+ ") + dstPath);
								CookingRules rules = null;
								if (!string.IsNullOrEmpty(dstPath)) {
									cookingRulesMap.TryGetValue(dstPath, out rules);
								}
								PluginLoader.AfterAssetUpdated(bundle, rules, dstPath);
							}
						} catch (System.Exception e) {
							Console.WriteLine(
								"An exception was caught while processing '{0}': {1}\n", srcPath, e.Message);
							throw;
						}
					} else {
						Console.WriteLine((bundled ? "* " : "+ ") + dstPath);
						using (Stream stream = new FileStream(srcPath, FileMode.Open, FileAccess.Read)) {
							bundle.ImportFile(dstPath, stream, 0, fileExtension, AssetAttributes.None, cookingRulesMap[srcPath].SHA1);
						}
					}
				}
			}
		}

		public class AtlasItem
		{
			public string Path;
			public Bitmap Bitmap;
			public IntRectangle AtlasRect;
			public bool Allocated;
			public CookingRules CookingRules;
			public string SourceExtension;
		}

		public static string GetAtlasPath(string atlasChain, int index)
		{
			var path = AssetPath.Combine(
				"Atlases" + atlasesPostfix, atlasChain + '.' + index.ToString("000") + GetPlatformTextureExtension());
			return path;
		}

		static void BuildAtlasChain(string atlasChain)
		{
			for (var i = 0; i < MaxAtlasChainLength; i++) {
				var atlasPath = GetAtlasPath(atlasChain, i);
				if (AssetBundle.FileExists(atlasPath)) {
					DeleteFileFromBundle(atlasPath);
				} else {
					break;
				}
			}
			var pluginItems = new Dictionary<string, List<AtlasItem>>();
			var items = new Dictionary<AtlasOptimization, List<AtlasItem>>();
			items[AtlasOptimization.Memory] = new List<AtlasItem>();
			items[AtlasOptimization.DrawCalls] = new List<AtlasItem>();
			foreach (var fileInfo in The.Workspace.AssetFiles.Enumerate(".png")) {
				var cookingRules = cookingRulesMap[fileInfo.Path];
				if (cookingRules.TextureAtlas == atlasChain) {
					var srcTexturePath = AssetPath.Combine(The.Workspace.AssetsDirectory, fileInfo.Path);
					Bitmap bitmap;
					using (var stream = File.OpenRead(srcTexturePath)) {
						bitmap = new Bitmap(stream);
					}
					if (ShouldDownscale(bitmap, cookingRules)) {
						bitmap = DownscaleTexture(bitmap, srcTexturePath, cookingRules);
					}
					// Ensure that no image exceeded maxAtlasSize limit
					DownscaleTextureToFitAtlas(ref bitmap, srcTexturePath);
					var item = new AtlasItem {
						Path = Path.ChangeExtension(fileInfo.Path, ".atlasPart"),
						Bitmap = bitmap,
						CookingRules = cookingRules,
						SourceExtension = Path.GetExtension(fileInfo.Path)
					};
					var k = cookingRules.AtlasPacker;
					if (!string.IsNullOrEmpty(k) && k != "Default") {
						List<AtlasItem> l;
						if (!pluginItems.TryGetValue(k, out l)) {
							pluginItems.Add(k, l = new List<AtlasItem>());
						}
						l.Add(item);
					} else {
						items[cookingRules.AtlasOptimization].Add(item);
					}
				}
			}
			var initialAtlasId = 0;
			foreach (var kv in items) {
				if (kv.Value.Any()) {
					if (Platform == TargetPlatform.iOS) {
						Predicate<PVRFormat> isRequireSquare = (format) => {
							return
								format == PVRFormat.PVRTC2 ||
								format == PVRFormat.PVRTC4 ||
								format == PVRFormat.PVRTC4_Forced;
						};
						var square = kv.Value.Where(item => isRequireSquare(item.CookingRules.PVRFormat)).ToList();
						var nonSquare = kv.Value.Where(item => !isRequireSquare(item.CookingRules.PVRFormat)).ToList();
						initialAtlasId = PackItemsToAtlas(atlasChain, square, kv.Key, initialAtlasId, true);
						initialAtlasId = PackItemsToAtlas(atlasChain, nonSquare, kv.Key, initialAtlasId, false);
					} else {
						initialAtlasId = PackItemsToAtlas(atlasChain, kv.Value, kv.Key, initialAtlasId, false);
					}
					foreach (var item in kv.Value) {
						item.Bitmap.Dispose();
					}
				}
			}
			var packers = PluginLoader.CurrentPlugin.AtlasPackers.ToDictionary(i => i.Metadata.Id, i => i.Value);
			foreach (var kv in pluginItems) {
				if (!packers.ContainsKey(kv.Key)) {
					throw new InvalidOperationException($"Packer {kv.Key} not found");
				}
				initialAtlasId = packers[kv.Key](atlasChain, kv.Value, initialAtlasId);
			}
		}

		private static int PackItemsToAtlas(string atlasChain, List<AtlasItem> items,
			AtlasOptimization atlasOptimization, int initialAtlasId, bool squareAtlas)
		{
			// Sort images in descending size order
			items.Sort((x, y) => {
				var a = Math.Max(x.Bitmap.Width, x.Bitmap.Height);
				var b = Math.Max(y.Bitmap.Width, y.Bitmap.Height);
				return b - a;
			});

			var atlasId = initialAtlasId;
			while (items.Count > 0) {
				if (atlasId >= MaxAtlasChainLength) {
					throw new Lime.Exception("Too many textures in the atlas chain {0}", atlasChain);
				}
				var bestSize = new Size(0, 0);
				double bestPackRate = 0;
				int minItemsLeft = Int32.MaxValue;

				// TODO: Fix for non-square atlases
				var maxTextureSize = items.Max(item => Math.Max(item.Bitmap.Height, item.Bitmap.Width));
				var minAtlasSize = Math.Max(64, CalcUpperPowerOfTwo(maxTextureSize));

				foreach (var size in EnumerateAtlasSizes(squareAtlas: squareAtlas, minSize: minAtlasSize)) {
					double packRate;
					var prevAllocated = items.Where(i => i.Allocated).ToList();
					PackItemsToAtlas(items, size, out packRate);
					switch (atlasOptimization) {
						case AtlasOptimization.Memory:
							if (packRate * 0.95f > bestPackRate) {
								bestPackRate = packRate;
								bestSize = size;
							}
							break;
						case AtlasOptimization.DrawCalls: {
							var notAllocatedCount = items.Count(item => !item.Allocated);
							if (notAllocatedCount < minItemsLeft) {
								minItemsLeft = notAllocatedCount;
								bestSize = size;
							} else if (notAllocatedCount == minItemsLeft) {
								if (items.Where(i => i.Allocated).SequenceEqual(prevAllocated)) {
									continue;
								} else {
									minItemsLeft = notAllocatedCount;
									bestSize = size;
								}
							}
							if (notAllocatedCount == 0) {
								goto end;
							}
							break;
						}
					}
				}
				end:
				if (atlasOptimization == AtlasOptimization.Memory && bestPackRate == 0) {
					throw new Lime.Exception("Failed to create atlas '{0}'", atlasChain);
				}
				PackItemsToAtlas(items, bestSize, out bestPackRate);
				CopyAllocatedItemsToAtlas(items, atlasChain, atlasId, bestSize);
				foreach (var x in items) {
					if (x.Allocated) {
						x.Bitmap.Dispose();
					}
				}
				items.RemoveAll(x => x.Allocated);
				atlasId++;
			}
			return atlasId;
		}

		private static int CalcUpperPowerOfTwo(int x)
		{
			x--;
			x |= (x >> 1);
			x |= (x >> 2);
			x |= (x >> 4);
			x |= (x >> 8);
			x |= (x >> 16);
			return (x + 1);
		}

		public static IEnumerable<Size> EnumerateAtlasSizes(bool squareAtlas, int minSize)
		{
			if (squareAtlas) {
				for (var i = minSize; i <= GetMaxAtlasSize().Width; i *= 2) {
					yield return new Size(i, i);
				}
			} else {
				for (var i = minSize; i <= GetMaxAtlasSize().Width / 2; i *= 2) {
					yield return new Size(i, i);
					yield return new Size(i * 2, i);
					yield return new Size(i, i * 2);
				}
				yield return GetMaxAtlasSize();
			}
		}

		private static Size GetMaxAtlasSize()
		{
			return new Size(2048, 2048);
		}

		private static void PackItemsToAtlas(List<AtlasItem> items, Size size, out double packRate)
		{
			items.ForEach(i => i.Allocated = false);
			// Take in account 1 pixel border for each side.
			var a = new RectAllocator(new Size(size.Width + 2, size.Height + 2));
			AtlasItem firstAllocatedItem = null;
			foreach (var item in items) {
				var sz = new Size(item.Bitmap.Width + 2, item.Bitmap.Height + 2);
				if (firstAllocatedItem == null || AreAtlasItemsCompatible(items, firstAllocatedItem, item)) {
					if (a.Allocate(sz, out item.AtlasRect)) {
						item.Allocated = true;
						firstAllocatedItem = firstAllocatedItem ?? item;
					}
				}
			}
			packRate = a.GetPackRate();
		}

		/// <summary>
		/// Checks whether two items can be packed to the same texture
		/// </summary>
		public static bool AreAtlasItemsCompatible(List<AtlasItem> items, AtlasItem item1, AtlasItem item2)
		{
			if (item1.CookingRules.WrapMode != item2.CookingRules.WrapMode) {
				return false;
			}
			if (item1.CookingRules.MinFilter != item2.CookingRules.MinFilter) {
				return false;
			}
			if (item1.CookingRules.MagFilter != item2.CookingRules.MagFilter) {
				return false;
			}
			if (item1.CookingRules.MipMaps != item2.CookingRules.MipMaps) {
				return false;
			}
			if (items.Count > 0) {
				if (item1.CookingRules.WrapMode != TextureWrapMode.Clamp || item2.CookingRules.WrapMode != TextureWrapMode.Clamp) {
					return false;
				}
			}
			switch (Platform) {
				case TargetPlatform.Android:
				case TargetPlatform.iOS:
					return item1.CookingRules.PVRFormat == item2.CookingRules.PVRFormat && item1.Bitmap.HasAlpha == item2.Bitmap.HasAlpha;
				case TargetPlatform.Win:
				case TargetPlatform.Mac:
					return item1.CookingRules.DDSFormat == item2.CookingRules.DDSFormat;
				case TargetPlatform.Unity:
					return true;
				default:
					throw new ArgumentException();
			}
		}

		private static void CopyAllocatedItemsToAtlas(List<AtlasItem> items, string atlasChain, int atlasId, Size size)
		{
			var atlasPath = GetAtlasPath(atlasChain, atlasId);
			var atlasPixels = new Color4[size.Width * size.Height];
			foreach (var item in items.Where(i => i.Allocated)) {
				CopyPixels(item.Bitmap, atlasPixels, item.AtlasRect.A.X, item.AtlasRect.A.Y, size.Width, size.Height);
				var atlasPart = new TextureAtlasElement.Params();
				atlasPart.AtlasRect = item.AtlasRect;
				atlasPart.AtlasRect.B -= new IntVector2(2, 2);
				atlasPart.AtlasPath = Path.ChangeExtension(atlasPath, null);
				Serialization.WriteObjectToBundle(AssetBundle, item.Path, atlasPart, Serialization.Format.Binary,
					item.SourceExtension, AssetAttributes.None, item.CookingRules.SHA1);
				// Delete non-atlased texture since now its useless
				var texturePath = Path.ChangeExtension(item.Path, GetPlatformTextureExtension());
				if (AssetBundle.FileExists(texturePath)) {
					DeleteFileFromBundle(texturePath);
				}
			}
			Console.WriteLine("+ " + atlasPath);
			var firstItem = items.First(i => i.Allocated);
			using (var atlas = new Bitmap(atlasPixels, size.Width, size.Height)) {
				ImportTexture(atlasPath, atlas, firstItem.CookingRules, CookingRulesSHA1: null);
			}
		}

		private static void CopyPixels(
			Bitmap source, Color4[] dstPixels, int dstX, int dstY, int dstWidth, int dstHeight)
		{
			if (source.Width > dstWidth - dstX || source.Height > dstHeight - dstY) {
				throw new Lime.Exception(
					"Unable to copy pixels. Source image runs out of the bounds of destination image.");
			}
			var srcPixels = source.GetPixels();
			// Make 1-pixel border around image by duplicating image edges
			for (int y = -1; y <= source.Height; y++) {
				int dstRow = y + dstY;
				if (dstRow < 0 || dstRow >= dstHeight) {
					continue;
				}
				int srcRow = y.Clamp(0, source.Height - 1);
				int srcOffset = srcRow * source.Width;
				int dstOffset = (y + dstY) * dstWidth + dstX;
				Array.Copy(srcPixels, srcOffset, dstPixels, dstOffset, source.Width);
				if (dstX > 0) {
					dstPixels[dstOffset - 1] = srcPixels[srcOffset];
				}
				if (dstX + source.Width < dstWidth) {
					dstPixels[dstOffset + source.Width] = srcPixels[srcOffset + source.Width - 1];
				}
			}
		}

		private static bool ShouldGenerateOpacityMasks()
		{
			return !The.Workspace.ProjectJson.GetValue("DontGenerateOpacityMasks", false);
		}

		public static bool AreTextureParamsDefault(ICookingRules rules)
		{
			return rules.MinFilter == TextureParams.Default.MinFilter &&
				rules.MagFilter == TextureParams.Default.MagFilter &&
				rules.WrapMode == TextureParams.Default.WrapModeU;
		}

		public static void ImportTexture(string path, Bitmap texture, ICookingRules rules, byte[] CookingRulesSHA1)
		{
			var textureParamsPath = Path.ChangeExtension(path, ".texture");
			if (!AreTextureParamsDefault(rules)) {
				UpscaleTextureIfNeeded(ref texture, rules, false);
				var textureParams = new TextureParams {
					WrapMode = rules.WrapMode,
					MinFilter = rules.MinFilter,
					MagFilter = rules.MagFilter,
				};
				Serialization.WriteObjectToBundle(AssetBundle, textureParamsPath, textureParams, Serialization.Format.Binary, ".texture", AssetAttributes.None, null);
			} else {
				if (AssetBundle.FileExists(textureParamsPath)) {
					DeleteFileFromBundle(textureParamsPath);
				}
			}
			if (ShouldGenerateOpacityMasks()) {
				var maskPath = Path.ChangeExtension(path, ".mask");
				OpacityMaskCreator.CreateMask(AssetBundle, texture, maskPath);
			}
			var attributes = AssetAttributes.ZippedDeflate;
			if (!TextureConverterUtils.IsPowerOf2(texture.Width) || !TextureConverterUtils.IsPowerOf2(texture.Height)) {
				attributes |= AssetAttributes.NonPowerOf2Texture;
			}
			switch (Platform) {
				case TargetPlatform.Android:
					var f = rules.PVRFormat;
					if (f == PVRFormat.ARGB8 || f == PVRFormat.RGB565 || f == PVRFormat.RGBA4) {
						TextureConverter.RunPVRTexTool(texture, AssetBundle, path, attributes, rules.MipMaps, rules.HighQualityCompression, rules.PVRFormat, CookingRulesSHA1);
					} else {
						TextureConverter.RunEtcTool(texture, AssetBundle, path, attributes, rules.MipMaps, rules.HighQualityCompression, CookingRulesSHA1);
					}
					break;
				case TargetPlatform.iOS:
					TextureConverter.RunPVRTexTool(texture, AssetBundle, path, attributes, rules.MipMaps, rules.HighQualityCompression, rules.PVRFormat, CookingRulesSHA1);
					break;
				case TargetPlatform.Win:
				case TargetPlatform.Mac:
					TextureConverter.RunNVCompress(texture, AssetBundle, path, attributes, rules.DDSFormat, rules.MipMaps, CookingRulesSHA1);
					break;
				default:
					throw new Lime.Exception();
			}
		}

		private static void UpscaleTextureIfNeeded(ref Bitmap texture, ICookingRules rules, bool square)
		{
			if (rules.WrapMode == TextureWrapMode.Clamp) {
				return;
			}
			if (TextureConverterUtils.IsPowerOf2(texture.Width) && TextureConverterUtils.IsPowerOf2(texture.Height)) {
				return;
			}
			int newWidth = CalcUpperPowerOfTwo(texture.Width);
			int newHeight = CalcUpperPowerOfTwo(texture.Height);
			if (square) {
				newHeight = newWidth = Math.Max(newWidth, newHeight);
			}
			var newTexture = texture.Rescale(newWidth, newHeight);
			texture.Dispose();
			texture = newTexture;
		}

		private static void DownscaleTextureToFitAtlas(ref Bitmap bitmap, string path)
		{
			var maxWidth = GetMaxAtlasSize().Width;
			var maxHeight = GetMaxAtlasSize().Height;
			if (bitmap.Width <= maxWidth && bitmap.Height <= maxHeight) {
				return;
			}
			var newWidth = Math.Min(bitmap.Width, maxWidth);
			var newHeight = Math.Min(bitmap.Height, maxHeight);
			var scaledBitmap = bitmap.Rescale(newWidth, newHeight);
			bitmap.Dispose();
			bitmap = scaledBitmap;
			Console.WriteLine($"WARNING: '{path}' downscaled to {newWidth}x{newHeight}");
		}

		private static bool ShouldDownscale(Bitmap texture, CookingRules rules)
		{
			if (rules.TextureScaleFactor != 1.0f) {
				int scaleThreshold = Platform == TargetPlatform.Android ? 32 : 256;
				if (texture.Width > scaleThreshold || texture.Height > scaleThreshold) {
					return true;
				}
			}
			return false;
		}

		private static Bitmap DownscaleTexture(Bitmap texture, string path, CookingRules rules)
		{
			int MaxSize = GetMaxAtlasSize().Width;
			int scaleThreshold = Platform == TargetPlatform.Android ? 32 : 256;
			var ratio = rules.TextureScaleFactor;
			if (texture.Width > MaxSize || texture.Height > MaxSize) {
				var max = (float) Math.Max(texture.Width, texture.Height);
				ratio *= MaxSize / max;
			}
			int newWidth = texture.Width;
			int newHeight = texture.Height;
			if (texture.Width > scaleThreshold) {
				newWidth = Math.Min((texture.Width * ratio).Round(), MaxSize);
			}
			if (texture.Height > scaleThreshold) {
				newHeight = Math.Min((texture.Height * ratio).Round(), MaxSize);
			}
			Console.WriteLine("{0} downscaled to {1}x{2}", path, newWidth, newHeight);
			return texture.Rescale(newWidth, newHeight);
		}

		static void SyncAtlases()
		{
			var textures = new Dictionary<string, DateTime>();
			foreach (var fileInfo in The.Workspace.AssetFiles.Enumerate(".png")) {
				textures[fileInfo.Path] = fileInfo.LastWriteTime;
			}
			var atlasChainsToRebuild = new HashSet<string>();
			// Figure out atlas chains to rebuild
			foreach (var atlasPartPath in AssetBundle.EnumerateFiles()) {
				if (Path.GetExtension(atlasPartPath) != ".atlasPart")
					continue;

				// If atlas part has been outdated we should rebuild full atlas chain
				var srcTexturePath = Path.ChangeExtension(atlasPartPath, ".png");
				var bundleSHA1 = AssetBundle.GetCookingRulesSHA1(atlasPartPath);
				if (bundleSHA1 == null) {
					throw new InvalidOperationException("CookingRules SHA1 for atlas part shouldn't be null");
				}
				if (
					!textures.ContainsKey(srcTexturePath) ||
					AssetBundle.GetFileLastWriteTime(atlasPartPath) < textures[srcTexturePath] ||
					(!cookingRulesMap[srcTexturePath].SHA1.SequenceEqual(bundleSHA1))
				) {
					srcTexturePath = AssetPath.Combine(The.Workspace.AssetsDirectory, srcTexturePath);
					var part = TextureAtlasElement.Params.ReadFromBundle(atlasPartPath);
					var atlasChain = Path.GetFileNameWithoutExtension(part.AtlasPath);
					atlasChainsToRebuild.Add(atlasChain);
					if (!textures.ContainsKey(srcTexturePath)) {
						DeleteFileFromBundle(atlasPartPath);
					} else {
						srcTexturePath = Path.ChangeExtension(atlasPartPath, ".png");
						if (cookingRulesMap[srcTexturePath].TextureAtlas != null) {
							var rules = cookingRulesMap[srcTexturePath];
							atlasChainsToRebuild.Add(rules.TextureAtlas);
						} else {
							DeleteFileFromBundle(atlasPartPath);
						}
					}
				}
			}
			// Find which new textures must be added to the atlas chain
			foreach (var t in textures) {
				var atlasPartPath = Path.ChangeExtension(t.Key, ".atlasPart");
				var cookingRules = cookingRulesMap[t.Key];
				var atlasNeedRebuld = cookingRules.TextureAtlas != null && !AssetBundle.FileExists(atlasPartPath);
				if (atlasNeedRebuld) {
					atlasChainsToRebuild.Add(cookingRules.TextureAtlas);
				}
			}
			foreach (var atlasChain in atlasChainsToRebuild) {
				CheckCookCancelation();
				BuildAtlasChain(atlasChain);
			}
		}

		private static void SyncModels()
		{
			var sourceAssetBundle = new UnpackedAssetBundle(The.Workspace.AssetsDirectory);
			SyncUpdated(".fbx", ".model", (srcPath, dstPath) => {
				var compression = cookingRulesMap[srcPath].ModelCompression;
				var model = new FbxModelImporter(srcPath, The.Workspace.ActivePlatform).Model;
				// Create .model file for tangerine.
				Serialization.WriteObjectToBundle(sourceAssetBundle, dstPath, model, Serialization.Format.Binary, ".model", AssetAttributes.None, cookingRulesMap[srcPath].SHA1);
				AssetAttributes assetAttributes;
				switch (compression) {
					case ModelCompression.None:
						assetAttributes = AssetAttributes.None;
						break;
					case ModelCompression.Deflate:
						assetAttributes = AssetAttributes.ZippedDeflate;
						break;
					case ModelCompression.LZMA:
						assetAttributes = AssetAttributes.ZippedLZMA;
						break;
					default:
						throw new ArgumentOutOfRangeException($"Unknown compression: {compression}");
				}
				AssetBundle.ImportFile(dstPath, dstPath, 0, ".model", assetAttributes, cookingRulesMap[srcPath].SHA1);
				return true;
			});
		}

		public static void CancelCook()
		{
			cookCanceled = true;
		}

		private static void CheckCookCancelation()
		{
			if (cookCanceled) {
				throw new OperationCanceledException("------------- Cooking canceled -------------");
			}
		}

		private static bool TryMakeBackup(string filePath, out string backupFilePath)
		{
			backupFilePath = filePath + ".bak";

			if (!File.Exists(filePath) ) {
				return false;
			}

			try {
				File.Copy(filePath, backupFilePath);
				return true;
			} catch (System.Exception e) {
				Console.WriteLine(e);
			}

			return false;
		}

		private static bool TryRestoreBackup(string backupFilePath)
		{
			if (!Path.GetExtension(backupFilePath).Equals(".bak")) {
				return false;
			}

			// Remove ".bak" extension.
			string targetFilePath = Path.ChangeExtension(backupFilePath, null);

			try {
				if (File.Exists(targetFilePath)) {
					File.Delete(targetFilePath);
				}
				File.Move(backupFilePath, targetFilePath);
				return true;
			} catch (System.Exception e) {
				Console.WriteLine(e);
			}

			return false;
		}

		private static void RemoveBackups()
		{
			foreach (var backupPath in bundleBackupFiles) {
				try {
					File.Delete(backupPath);
				} catch (System.Exception e) {
					Console.WriteLine(e);
				}
			}
			bundleBackupFiles.Clear();
		}

		private static void RestoreBackups()
		{
			foreach (var backupPath in bundleBackupFiles) {
				TryRestoreBackup(backupPath);
			}
		}
	}
}
