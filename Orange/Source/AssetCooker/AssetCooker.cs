using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using Lime;
using Orange.FbxImporter;
using Debug = System.Diagnostics.Debug;

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
		private static readonly Dictionary<ICookStage, CookingProfile[]> cookStages = new Dictionary<ICookStage, CookingProfile[]>();
		private static CookingProfile cookingProfile = CookingProfile.Total;
		public static IEnumerable<ICookStage> CookStages => cookStages.Keys;

		private delegate bool Converter(string srcPath, string dstPath);

		public static AssetBundle AssetBundle => AssetBundle.Current;
		public static TargetPlatform Platform;
		public static Dictionary<string, CookingRules> CookingRulesMap;
		public static HashSet<string> ModelsToRebuild = new HashSet<string>();

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

		public static void AddStage(ICookStage stage, params CookingProfile[] cookingProfiles)
		{
			cookStages.Add(stage, cookingProfiles.Length == 0 ? defaultCookingProfiles : cookingProfiles);
		}

		public static void RemoveStage(ICookStage stage)
		{
			cookStages.Remove(stage);
		}

		public static string GetOriginalAssetExtension(string path)
		{
			var ext = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);
			switch (ext) {
			case ".dds":
			case ".pvr":
			case ".atlasPart":
			case ".mask":
			case ".jpg":
				return ".png";
			case ".sound":
				return ".ogg";
			case ".t3d":
				return ".fbx";
			default:
				return ext;
			}
		}

		public static string GetPlatformTextureExtension()
		{
			switch (Platform) {
				case TargetPlatform.iOS:
				case TargetPlatform.Android:
					return ".pvr";
				default:
					return ".dds";
			}
		}

		public static void Cook(TargetPlatform platform, List<string> bundles = null)
		{
			AssetCache.Instance.Initialize();
			AssetCooker.Platform = platform;
			CookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, The.Workspace.ActiveTarget);
			if (The.Workspace.BenchmarkEnabled) {
				string text = $"Asset cooking. Enabled cache: {AssetCache.Instance.EnableState}. Active platform: {The.Workspace.ActivePlatform}";
				Debug.WriteLine(text);
				using (var w = File.AppendText(Path.Combine(The.Workspace.ProjectDirectory, "cache.log"))) {
					w.WriteLine(text);
					w.WriteLine(DateTime.Now);
				}
				Stopwatch timer = new Stopwatch();
				timer.Start();
				CookBundles(bundles: bundles);
				timer.Stop();
				text = $"All bundles cooked: {timer.ElapsedMilliseconds} ms";
				Debug.WriteLine(text);
				using (var w = File.AppendText(Path.Combine(The.Workspace.ProjectDirectory, "cache.log"))) {
					w.WriteLine(text);
					w.WriteLine();
					w.WriteLine();
				}
			} else {
				CookBundles(bundles: bundles);
			}
		}

		public static void CookCustomAssets(TargetPlatform platform, List<string> assets)
		{
			AssetCooker.Platform = platform;
			CookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, The.Workspace.ActiveTarget);

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
			bool skipCodeCooking = The.Workspace.ProjectJson.GetValue<bool>("SkipCodeCooking");
			if (skipCodeCooking) {
				requiredCookCode = false;
			}

			var extraBundles = new HashSet<string>();
			foreach (var dictionaryItem in CookingRulesMap) {
				foreach (var bundle in dictionaryItem.Value.Bundles) {
					if (bundle != CookingRulesBuilder.MainBundleName && (bundles == null || bundles.Contains(bundle))) {
						extraBundles.Add(bundle);
					}
				}
			}

			try {			
				int s = 0;
				// Drop cooking rules, they shouldn't be counting as assets, but The.Workspace.AssetFiles includes them
				foreach (var asset in The.Workspace.AssetFiles.Enumerate()) {
					if (string.Equals(Path.GetExtension(asset.Path), ".txt", StringComparison.OrdinalIgnoreCase)) {
						if (string.Equals(Path.GetFileName(asset.Path), "#CookingRules.txt", StringComparison.OrdinalIgnoreCase) ||
							!string.Equals(Path.GetExtension(Path.GetFileNameWithoutExtension(asset.Path)), string.Empty, StringComparison.OrdinalIgnoreCase))
							continue;
					}
					s++;
				}
				
				UserInterface.Instance.SetupProgressBar(s);
				BeginCookBundles?.Invoke();

				if (The.Workspace.BenchmarkEnabled) {
					Stopwatch timer = new Stopwatch();
					timer.Start();
					CookBundle(CookingRulesBuilder.MainBundleName);
					timer.Stop();
					string text = $"Main bundle cooked: {timer.ElapsedMilliseconds} ms";
					Debug.WriteLine(text);
					using (var w = File.AppendText(Path.Combine(The.Workspace.ProjectDirectory, "cache.log"))) {
						w.WriteLine(text);
					}
				} else {
					CookBundle(CookingRulesBuilder.MainBundleName);
				}

				foreach (var extraBundle in extraBundles) {
					if (The.Workspace.BenchmarkEnabled) {
						Stopwatch extraTimer = new Stopwatch();
						extraTimer.Start();
						CookBundle(extraBundle);
						extraTimer.Stop();
						string extraText = $"{extraBundle} cooked: {extraTimer.ElapsedMilliseconds} ms";
						Debug.WriteLine(extraText);
						using (var w = File.AppendText(Path.Combine(The.Workspace.ProjectDirectory, "cache.log"))) {
							w.WriteLine(extraText);
						}
					} else {
						CookBundle(extraBundle);
					}
				}
				extraBundles.Add(CookingRulesBuilder.MainBundleName);

				var extraBundlesList = extraBundles.Reverse().ToList();
				PluginLoader.AfterBundlesCooked(extraBundlesList);
				if (requiredCookCode) {
					CodeCooker.Cook(CookingRulesMap, extraBundlesList);
				}
			} catch (OperationCanceledException e) {
				Console.WriteLine(e.Message);
				RestoreBackups();
			} finally {
				cookCanceled = false;
				RemoveBackups();
				EndCookBundles?.Invoke();
				UserInterface.Instance.StopProgressBar();
			}
		}

		private static void CookBundle(string bundleName)
		{
			string bundlePath = The.Workspace.GetBundlePath(bundleName);
			bool wasBundleModified = false;
			using (var bundle = CreateBundle(bundleName)) {
				AssetBundle.SetCurrent(bundle, false);
				(AssetBundle.Current as PackedAssetBundle).OnModifying += () => {
					if (!wasBundleModified) {
						wasBundleModified = true;
						string backupFilePath;
						TryMakeBackup(bundlePath, out backupFilePath);
						bundleBackupFiles.Add(backupFilePath);
					}
				};
				CookBundleHelper(bundleName);
				// Open the bundle again in order to make some plugin post-processing (e.g. generate code from scene assets)
				using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
					PluginLoader.AfterAssetsCooked(bundleName);
				}
			}
			if (wasBundleModified) {
				PackedAssetBundle.RefreshBundleCheckSum(bundlePath);
			}
		}

		private static AssetBundle CreateBundle(string bundleName)
		{
			var bundlePath = The.Workspace.GetBundlePath(bundleName);
			// Create directory for bundle if it placed in subdirectory
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(bundlePath));
			} catch (System.Exception) {
				Lime.Debug.Write("Failed to create directory: {0} {1}", Path.GetDirectoryName(bundlePath));
				throw;
			}

			return new PackedAssetBundle(bundlePath, AssetBundleFlags.Writable);
		}

		private class FilteredFileEnumerator : IFileEnumerator
		{
			private IFileEnumerator sourceFileEnumerator;
			private List<FileInfo> files = new List<FileInfo>();
			public FilteredFileEnumerator(IFileEnumerator fileEnumerator, Predicate<FileInfo> predicate)
			{
				sourceFileEnumerator = fileEnumerator;
				sourceFileEnumerator.EnumerationFilter = predicate;
				files = sourceFileEnumerator.Enumerate().ToList();
				sourceFileEnumerator.EnumerationFilter = null;
			}
			public string Directory { get { return sourceFileEnumerator.Directory; } }

			public Predicate<FileInfo> EnumerationFilter { get { return null; } set { } }

			public IEnumerable<FileInfo> Enumerate(string extension = null)
			{
				return files.Where(file => extension == null || file.Path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
			}

			public void Rescan()
			{
				
			}
		}

		private static void CookBundleHelper(string bundleName)
		{
			Console.WriteLine("------------- Cooking Assets ({0}) -------------", bundleName);
			var assetFilesEnumerator = The.Workspace.AssetFiles;
			The.Workspace.AssetFiles = new FilteredFileEnumerator(assetFilesEnumerator, (info) => {
				CookingRules rules;
				if (CookingRulesMap.TryGetValue(info.Path, out rules)) {
					if (rules.Ignore)
						return false;
					return Array.IndexOf(rules.Bundles, bundleName) != -1;
				} else {
					// There are no cooking rules for text files, consider them as part of the main bundle.
					return bundleName == CookingRulesBuilder.MainBundleName;
				}
			});
			// Every asset bundle must have its own atlases folder, so they aren't conflict with each other
			atlasesPostfix = bundleName != CookingRulesBuilder.MainBundleName ? bundleName : "";
			try {
				using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
					var profileCookStages = cookStages
						.Where(kv => kv.Value.Contains(cookingProfile))
						.Select(kv => kv.Key);
					foreach (var stage in profileCookStages) {
						CheckCookCancelation();
						stage.Action();
					}

					// Warn about non-power of two textures
					foreach (var path in AssetBundle.EnumerateFiles()) {
						if ((AssetBundle.GetAttributes(path) & AssetAttributes.NonPowerOf2Texture) != 0) {
							Console.WriteLine("Warning: non-power of two texture: {0}", path);
						}
					}
				}
			} finally {
				The.Workspace.AssetFiles = assetFilesEnumerator;
				ModelsToRebuild.Clear();
				atlasesPostfix = "";
			}
		}

		static AssetCooker()
		{
			bundleBackupFiles = new List<String>();

			AddStage(new RemoveDeprecatedModels());
			AddStage(new SyncAtlases(), CookingProfile.Total);
			AddStage(new SyncDeleted(), CookingProfile.Total);
			AddStage(new SyncRawAssets(".json", AssetAttributes.ZippedDeflate));
			AddStage(new SyncTxtAssets());
			AddStage(new SyncRawAssets(".csv", AssetAttributes.ZippedDeflate));
			var rawAssetExtensions = The.Workspace.ProjectJson["RawAssetExtensions"] as string;
			if (rawAssetExtensions != null) {
				foreach (var extension in rawAssetExtensions.Split(' ')) {
					AddStage(new SyncRawAssets(extension, AssetAttributes.ZippedDeflate));
				}
			}
			AddStage(new SyncTextures(), CookingProfile.Total);
			AddStage(new DeleteOrphanedMasks(), CookingProfile.Total);
			AddStage(new DeleteOrphanedTextureParams(), CookingProfile.Total);
			AddStage(new SyncFonts());
			AddStage(new SyncHotFonts());
			AddStage(new SyncCompoundFonts());
			AddStage(new SyncRawAssets(".ttf"));
			AddStage(new SyncRawAssets(".otf"));
			AddStage(new SyncRawAssets(".ogv"));
			AddStage(new SyncScenes());
			AddStage(new SyncHotScenes());
			AddStage(new SyncSounds());
			AddStage(new SyncRawAssets(".shader"));
			AddStage(new SyncRawAssets(".xml"));
			AddStage(new SyncRawAssets(".raw"));
			AddStage(new SyncRawAssets(".bin"));
			AddStage(new SyncModels());
		}

		public static void DeleteFileFromBundle(string path)
		{
			Console.WriteLine("- " + path);
			AssetBundle.DeleteFile(path);
		}

		public static string GetAtlasPath(string atlasChain, int index)
		{
			var path = AssetPath.Combine(
				"Atlases" + atlasesPostfix, atlasChain + '.' + index.ToString("000") + GetPlatformTextureExtension());
			return path;
		}

		public static bool AreTextureParamsDefault(ICookingRules rules)
		{
			return rules.MinFilter == TextureParams.Default.MinFilter &&
				rules.MagFilter == TextureParams.Default.MagFilter &&
				rules.WrapMode == TextureParams.Default.WrapModeU;
		}

		public static void ImportTexture(string path, Bitmap texture, ICookingRules rules, DateTime time, byte[] CookingRulesSHA1)
		{
			var textureParamsPath = Path.ChangeExtension(path, ".texture");
			var textureParams = new TextureParams {
				WrapMode = rules.WrapMode,
				MinFilter = rules.MinFilter,
				MagFilter = rules.MagFilter,
			};

			if (!AreTextureParamsDefault(rules)) {
				TextureTools.UpscaleTextureIfNeeded(ref texture, rules, false);
				var isNeedToRewriteTexParams = true;
				if (AssetBundle.FileExists(textureParamsPath)) {
					var oldTexParams = Serialization.ReadObject<TextureParams>(textureParamsPath, AssetBundle.OpenFile(textureParamsPath));
					isNeedToRewriteTexParams = !oldTexParams.Equals(textureParams);
				}
				if (isNeedToRewriteTexParams) {
					Serialization.WriteObjectToBundle(AssetBundle, textureParamsPath, textureParams, Serialization.Format.Binary, ".texture",
						File.GetLastWriteTime(textureParamsPath), AssetAttributes.None, null);
				}
			} else {
				if (AssetBundle.FileExists(textureParamsPath)) {
					DeleteFileFromBundle(textureParamsPath);
				}
			}
			if (rules.GenerateOpacityMask) {
				var maskPath = Path.ChangeExtension(path, ".mask");
				OpacityMaskCreator.CreateMask(AssetBundle, texture, maskPath);
			}
			var attributes = AssetAttributes.ZippedDeflate;
			if (!TextureConverterUtils.IsPowerOf2(texture.Width) || !TextureConverterUtils.IsPowerOf2(texture.Height)) {
				attributes |= AssetAttributes.NonPowerOf2Texture;
			}
			switch (Platform) {
				case TargetPlatform.Android:
				//case TargetPlatform.iOS:
					var f = rules.PVRFormat;
					if (f == PVRFormat.ARGB8 || f == PVRFormat.RGB565 || f == PVRFormat.RGBA4) {
						TextureConverter.RunPVRTexTool(texture, AssetBundle, path, attributes, rules.MipMaps, rules.HighQualityCompression, rules.PVRFormat, CookingRulesSHA1, time);
					} else {
						TextureConverter.RunEtcTool(texture, AssetBundle, path, attributes, rules.MipMaps, rules.HighQualityCompression, CookingRulesSHA1, time);
					}
					break;
				case TargetPlatform.iOS:
					TextureConverter.RunPVRTexTool(texture, AssetBundle, path, attributes, rules.MipMaps, rules.HighQualityCompression, rules.PVRFormat, CookingRulesSHA1, time);
					break;
				case TargetPlatform.Win:
				case TargetPlatform.Mac:
					TextureConverter.RunNVCompress(texture, AssetBundle, path, attributes, rules.DDSFormat, rules.MipMaps, CookingRulesSHA1, time);
					break;
				default:
					throw new Lime.Exception();
			}
		}

		public static void DeleteModelExternalAnimations(string pathPrefix)
		{
			foreach (var path in AssetBundle.EnumerateFiles().ToList()) {
				if (path.EndsWith(".ant") && path.StartsWith(pathPrefix)) {
					AssetBundle.DeleteFile(path);
					Console.WriteLine("- " + path);
				}
			}
		}

		public static void ExportModelAnimations(Model3D model, string pathPrefix, AssetAttributes assetAttributes, byte[] cookingRulesSHA1)
		{
			foreach (var animation in model.Animations) {
				if (animation.IsLegacy) {
					continue;
				}
				var pathWithoutExt = pathPrefix + animation.Id;
				pathWithoutExt = Animation.FixAntPath(pathWithoutExt);
				var path = pathWithoutExt + ".ant";
				var data = animation.GetData();
				animation.ContentsPath = pathWithoutExt;
				Serialization.WriteObjectToBundle(AssetBundle, path, data, Serialization.Format.Binary, ".ant", File.GetLastWriteTime(path), assetAttributes, cookingRulesSHA1);
				Console.WriteLine("+ " + path);
			}
		}

		public static string GetModelAnimationPathPrefix(string modelPath)
		{
			return Toolbox.ToUnixSlashes(Path.GetDirectoryName(modelPath) + "/" + Path.GetFileNameWithoutExtension(modelPath) + "@");
		}

		public static void CancelCook()
		{
			cookCanceled = true;
		}

		public static void CheckCookCancelation()
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
			if (!backupFilePath.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)) {
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
					Console.WriteLine("Failed to delete backupFile: {0} {1}", backupPath, e);
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
