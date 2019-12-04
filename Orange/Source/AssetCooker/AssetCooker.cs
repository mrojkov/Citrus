using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using Lime;
using Orange.FbxImporter;

namespace Orange
{
	public enum CookingProfile
	{
		Total,
		Partial
	}

	public class AssetCooker
	{
		private static readonly CookingProfile[] defaultCookingProfiles = { CookingProfile.Total, CookingProfile.Partial };
		private readonly Dictionary<ICookStage, CookingProfile[]> cookStages = new Dictionary<ICookStage, CookingProfile[]>();
		private static CookingProfile cookingProfile = CookingProfile.Total;
		public IEnumerable<ICookStage> CookStages => cookStages.Keys;

		private delegate bool Converter(string srcPath, string dstPath);

		public AssetBundle AssetBundle => AssetBundle.Current;
		//public static TargetPlatform Platform;
		public static Dictionary<string, CookingRules> CookingRulesMap;
		public static HashSet<string> ModelsToRebuild = new HashSet<string>();

		private static string atlasesPostfix = string.Empty;

		public const int MaxAtlasChainLength = 1000;

		public static event Action BeginCookBundles;
		public static event Action EndCookBundles;

		private static bool cookCanceled = false;
		private ICollection<string> bundleBackupFiles;

		public readonly Target Target;
		public TargetPlatform Platform => Target.Platform;

		public static void CookForTarget(Target target, IEnumerable<string> bundles = null)
		{
			var assetCooker = new AssetCooker(target);
			var skipCooking = The.Workspace.ProjectJson.GetValue<bool>("SkipAssetsCooking");
			if (!skipCooking) {
				assetCooker.Cook(bundles ?? assetCooker.GetListOfAllBundles());
			} else {
				Console.WriteLine("-------------  Skip Assets Cooking -------------");
			}
		}

		public void AddStage(ICookStage stage, params CookingProfile[] cookingProfiles)
		{
			cookStages.Add(stage, cookingProfiles.Length == 0 ? defaultCookingProfiles : cookingProfiles);
		}

		public void RemoveStage(ICookStage stage)
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

		public List<string> GetListOfAllBundles()
		{
			var cookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, Target);
			var bundles = new HashSet<string>();
			foreach (var dictionaryItem in cookingRulesMap) {
				foreach (var bundle in dictionaryItem.Value.Bundles) {
					bundles.Add(bundle);
				}
			}
			return bundles.ToList();
		}

		public string GetPlatformTextureExtension()
		{
			switch (Target.Platform) {
				case TargetPlatform.iOS:
				case TargetPlatform.Android:
					return ".pvr";
				default:
					return ".dds";
			}
		}

		public void Cook(IEnumerable<string> bundles)
		{
			AssetCache.Instance.Initialize();
			CookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, Target);
			CookBundles(bundles);
		}

		public void CookCustomAssets(List<string> assets)
		{
			CookingRulesMap = CookingRulesBuilder.Build(The.Workspace.AssetFiles, Target);

			var defaultAssetsEnumerator = The.Workspace.AssetFiles;
			var assetsFileInfo = assets
				.Select(asset => new FileInfo { Path = asset, LastWriteTime = DateTime.Now })
				.ToList();
			The.Workspace.AssetFiles = new CustomFilesEnumerator(defaultAssetsEnumerator.Directory, assetsFileInfo);

			var defaultCookingProfile = AssetCooker.cookingProfile;
			AssetCooker.cookingProfile = CookingProfile.Partial;

			CookBundles(GetListOfAllBundles(), false);
			The.Workspace.AssetFiles = defaultAssetsEnumerator;
			AssetCooker.cookingProfile = defaultCookingProfile;
		}

		private void CookBundles(IEnumerable<string> bundles, bool requiredCookCode = true)
		{
			LogText = "";
			var allTimer = StartBenchmark(
				$"Asset cooking. Asset cache mode: {AssetCache.Instance.Mode}. Active platform: {Target.Platform}" +
				System.Environment.NewLine +
				DateTime.Now +
				System.Environment.NewLine
			);

			PluginLoader.BeforeBundlesCooking();

			bool skipCodeCooking = The.Workspace.ProjectJson.GetValue<bool>("SkipCodeCooking");
			if (skipCodeCooking) {
				requiredCookCode = false;
			}

			try {
				UserInterface.Instance.SetupProgressBar(CalculateAssetCount(bundles));
				BeginCookBundles?.Invoke();

				foreach (var bundle in bundles) {
					var extraTimer = StartBenchmark();
					CookBundle(bundle);
					StopBenchmark(extraTimer, $"{bundle} cooked: ");
				}

				var extraBundles = bundles.ToList();
				extraBundles.Remove(CookingRulesBuilder.MainBundleName);
				extraBundles.Reverse();
				PluginLoader.AfterBundlesCooked(extraBundles);
				if (requiredCookCode) {
					CodeCooker.Cook(Target, CookingRulesMap, bundles.ToList());
				}
				StopBenchmark(allTimer, "All bundles cooked: ");
				PrintBenchmark();
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

		private int CalculateAssetCount(IEnumerable<string> bundles)
		{
			var assetCount = 0;
			var savedWorkspaceAssetFiles = The.Workspace.AssetFiles;
			foreach (var bundleName in bundles) {
				using (var bundle = CreateBundle(bundleName)) {
					AssetBundle.SetCurrent(bundle, false);
					The.Workspace.AssetFiles = new FilteredFileEnumerator(savedWorkspaceAssetFiles, (info) => AssetIsInBundlePredicate(info, bundleName));
					using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
						var profileCookStages = cookStages
							.Where(kv => kv.Value.Contains(cookingProfile))
							.Select(kv => kv.Key);
						foreach (var stage in profileCookStages) {
							assetCount += stage.GetOperationsCount();
						}
					}
				}
			}
			The.Workspace.AssetFiles = savedWorkspaceAssetFiles;
			return assetCount;
		}

		private void CookBundle(string bundleName)
		{
			string bundlePath = The.Workspace.GetBundlePath(Target.Platform, bundleName);
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

		private AssetBundle CreateBundle(string bundleName)
		{
			var bundlePath = The.Workspace.GetBundlePath(Target.Platform, bundleName);
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

		private static bool AssetIsInBundlePredicate(FileInfo info, string bundleName)
		{
			CookingRules rules;
			if (CookingRulesMap.TryGetValue(info.Path, out rules)) {
				if (rules.Ignore) {
					return false;
				}
				return Array.IndexOf(rules.Bundles, bundleName) != -1;
			} else {
				// There are no cooking rules for text files, consider them as part of the main bundle.
				return bundleName == CookingRulesBuilder.MainBundleName;
			}
		}

		private void CookBundleHelper(string bundleName)
		{
			Console.WriteLine("------------- Cooking Assets ({0}) -------------", bundleName);
			var assetFilesEnumerator = The.Workspace.AssetFiles;
			The.Workspace.AssetFiles = new FilteredFileEnumerator(assetFilesEnumerator, (info) => AssetIsInBundlePredicate(info, bundleName));
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

		public AssetCooker(Target target)
		{
			this.Target = target;
			bundleBackupFiles = new List<String>();

			AddStage(new RemoveDeprecatedModels(this));
			AddStage(new SyncAtlases(this), CookingProfile.Total);
			AddStage(new SyncDeleted(this), CookingProfile.Total);
			AddStage(new SyncRawAssets(this, ".json", AssetAttributes.ZippedDeflate));
			AddStage(new SyncRawAssets(this, ".cfg", AssetAttributes.ZippedDeflate));
			AddStage(new SyncTxtAssets(this));
			AddStage(new SyncRawAssets(this, ".csv", AssetAttributes.ZippedDeflate));
			var rawAssetExtensions = The.Workspace.ProjectJson["RawAssetExtensions"] as string;
			if (rawAssetExtensions != null) {
				foreach (var extension in rawAssetExtensions.Split(' ')) {
					AddStage(new SyncRawAssets(this, extension, AssetAttributes.ZippedDeflate));
				}
			}
			AddStage(new SyncTextures(this), CookingProfile.Total);
			AddStage(new DeleteOrphanedMasks(this), CookingProfile.Total);
			AddStage(new DeleteOrphanedTextureParams(this), CookingProfile.Total);
			AddStage(new SyncFonts(this));
			AddStage(new SyncCompoundFonts(this));
			AddStage(new SyncRawAssets(this, ".ttf"));
			AddStage(new SyncRawAssets(this, ".otf"));
			AddStage(new SyncRawAssets(this, ".ogv"));
			AddStage(new SyncScenes(this));
			AddStage(new SyncSounds(this));
			AddStage(new SyncRawAssets(this, ".shader"));
			AddStage(new SyncRawAssets(this, ".xml"));
			AddStage(new SyncRawAssets(this, ".raw"));
			AddStage(new SyncRawAssets(this, ".bin"));
			AddStage(new SyncModels(this));
		}

		public void DeleteFileFromBundle(string path)
		{
			Console.WriteLine("- " + path);
			AssetBundle.DeleteFile(path);
		}

		public string GetAtlasPath(string atlasChain, int index)
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

		public void ImportTexture(string path, Bitmap texture, ICookingRules rules, DateTime time, byte[] CookingRulesSHA1)
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
					var oldTexParams = InternalPersistence.Instance.ReadObject<TextureParams>(textureParamsPath, AssetBundle.OpenFile(textureParamsPath));
					isNeedToRewriteTexParams = !oldTexParams.Equals(textureParams);
				}
				if (isNeedToRewriteTexParams) {
					InternalPersistence.Instance.WriteObjectToBundle(AssetBundle, textureParamsPath, textureParams, Persistence.Format.Binary, ".texture",
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
			switch (Target.Platform) {
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

		public void DeleteModelExternalAnimations(string pathPrefix)
		{
			foreach (var path in AssetBundle.EnumerateFiles().ToList()) {
				if (path.EndsWith(".ant") && path.StartsWith(pathPrefix)) {
					AssetBundle.DeleteFile(path);
					Console.WriteLine("- " + path);
				}
			}
		}

		public void ExportModelAnimations(Model3D model, string pathPrefix, AssetAttributes assetAttributes, byte[] cookingRulesSHA1)
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
				InternalPersistence.Instance.WriteObjectToBundle(AssetBundle, path, data, Persistence.Format.Binary, ".ant", File.GetLastWriteTime(path), assetAttributes, cookingRulesSHA1);
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

		private void RemoveBackups()
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

		private void RestoreBackups()
		{
			foreach (var backupPath in bundleBackupFiles) {
				TryRestoreBackup(backupPath);
			}
		}

		private static string LogText;
		private static Stopwatch StartBenchmark(string text="")
		{
			if (!The.Workspace.BenchmarkEnabled) {
				return null;
			}
			LogText += text;
			var timer = new Stopwatch();
			timer.Start();
			return timer;
		}

		private static void StopBenchmark(Stopwatch timer, string text)
		{
			if (!The.Workspace.BenchmarkEnabled) {
				return;
			}
			timer.Stop();
			LogText += text + $"{timer.ElapsedMilliseconds} ms" + System.Environment.NewLine;
		}

		private static void PrintBenchmark()
		{
			if (!The.Workspace.BenchmarkEnabled) {
				return;
			}
			using (var w = File.AppendText(Path.Combine(The.Workspace.ProjectDirectory, "cache.log"))) {
				w.WriteLine(LogText);
				w.WriteLine();
				w.WriteLine();
			}
		}
	}
}
