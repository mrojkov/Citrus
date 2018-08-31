using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Orange
{
	public class Workspace
	{
		public string ProjectFile { get; private set; }
		public string ProjectDirectory { get; private set; }
		public string AssetsDirectory { get; private set; }
		public string Title { get; private set; }
		public IFileEnumerator AssetFiles { get; set; }
		public Json ProjectJson { get; private set; }
		public List<Target> Targets { get; private set; }
		public string TangerineCacheBundle { get; private set; }

		private string dataFolderName;
		private string pluginName;
		private string citrusLocation;

		public Workspace()
		{
			Targets = new List<Target>();
			FillDefaultTargets();
		}

		public string GetPlatformSuffix(TargetPlatform? platform = null)
		{
			return "." + (platform?.ToString() ?? ActivePlatform.ToString());
		}

		/// <summary>
		/// Returns solution path. E.g: Zx3.Win/Zx3.Win.sln
		/// </summary>
		public string GetSolutionFilePath(TargetPlatform? platform = null)
		{
			string platformProjectName = The.Workspace.Title + GetPlatformSuffix(platform);
			return Path.Combine(
				The.Workspace.ProjectDirectory,
				platformProjectName,
				platformProjectName + ".sln");
		}

		/// <summary>
		/// Returns main project path. E.g: Zx3.Win/Zx3.Win.csproj
		/// </summary>
		public string GetMainCsprojFilePath()
		{
			return Path.ChangeExtension(GetSolutionFilePath(), ".csproj");
		}

		/// <summary>
		/// Returns Citrus/Lime project path.
		/// </summary>
		public string GetLimeCsprojFilePath(TargetPlatform? platform = null)
		{
			if (platform == null) {
				platform = The.Workspace.ActivePlatform;
			}
			// Now Citrus can either be located beside the game or inside the game directory.
			// In future projects Citrus location should be specified through "CitrusLocation" in citproj config file
			var suffix = Path.Combine("Lime", "Lime" + GetPlatformSuffix(platform) + ".csproj");
			string result;
			if (!string.IsNullOrEmpty(citrusLocation)) {
				result = Path.Combine(ProjectDirectory, citrusLocation, suffix);
			} else {
				result = Path.Combine(Path.GetDirectoryName(ProjectDirectory), "Citrus", suffix);
				if (!File.Exists(result)) {
					result = Path.Combine(ProjectDirectory, "Citrus", suffix);
				}
			}
			return result;
		}

		public static readonly Workspace Instance = new Workspace();

		public TargetPlatform ActivePlatform => ActiveTarget.Platform;

		public Target ActiveTarget => The.UI.GetActiveTarget();

		public string CustomSolution => ActiveTarget?.ProjectPath;

		public bool CleanBeforeBuild => (ActiveTarget?.CleanBeforeBuild == true);

		public JObject JObject { get; private set; }

		public void Load()
		{
			var config = WorkspaceConfig.Load();
			Open(config.CitrusProject);
			The.UI.LoadFromWorkspaceConfig(config);
			var citrusVersion = CitrusVersion.Load();
			if (citrusVersion.IsStandalone) {
				Console.WriteLine($"Welcome to Citrus. Version {citrusVersion.Version}, build number: {citrusVersion.BuildNumber}");
			}
#pragma warning disable CS4014
			Orange.Updater.CheckForUpdates();
#pragma warning restore CS4014
		}

		public void Save()
		{
			var config = WorkspaceConfig.Load();
			config.CitrusProject = ProjectFile;
			The.UI.SaveToWorkspaceConfig(ref config);
			WorkspaceConfig.Save(config);
		}

		public void Open(string file)
		{
			try {
				The.UI.ClearLog();
				ProjectFile = file;
				ReadProject(file);
				ProjectDirectory = Path.GetDirectoryName(file);
				AssetsDirectory = Path.Combine(ProjectDirectory, dataFolderName);
				if (!Directory.Exists(AssetsDirectory)) {
					throw new Lime.Exception("Assets folder '{0}' doesn't exist", AssetsDirectory);
				}
				PluginLoader.ScanForPlugins(!string.IsNullOrWhiteSpace(pluginName)
					? Path.Combine(Path.GetDirectoryName(file), pluginName)
					: file);
				if (defaultCsprojSynchronizationSkipUnwantedDirectoriesPredicate == null) {
					defaultCsprojSynchronizationSkipUnwantedDirectoriesPredicate = CsprojSynchronization.SkipUnwantedDirectoriesPredicate;
				}
				CsprojSynchronization.SkipUnwantedDirectoriesPredicate = (di) => {
					return defaultCsprojSynchronizationSkipUnwantedDirectoriesPredicate(di) && !di.FullName.StartsWith(AssetsDirectory, StringComparison.OrdinalIgnoreCase);
				};
				AssetFiles = new FileEnumerator(AssetsDirectory);
				TangerineCacheBundle = GetTangerineCacheBundlePath();
				The.UI.OnWorkspaceOpened();
			}
			catch (System.Exception e) {
				Console.WriteLine($"Can't open {file}:\n{e.Message}");
			}
		}

		// Preserving default targets references just in case since they're used as keys in cooking rules for target
		private static List<Target> defaultTargets;
		private Predicate<DirectoryInfo> defaultCsprojSynchronizationSkipUnwantedDirectoriesPredicate;

		private void FillDefaultTargets()
		{
			if (defaultTargets == null) {
				defaultTargets = new List<Target>();
				foreach (TargetPlatform platform in Enum.GetValues(typeof(TargetPlatform))) {
					defaultTargets.Add(new Target(Enum.GetName(typeof(TargetPlatform), platform), null, false, platform));
				}
			}
			Targets.AddRange(defaultTargets);
		}

		private void ReadProject(string file)
		{
			ProjectJson = new Json(file);
			Title = ProjectJson["Title"] as string;
			Targets = new List<Target>();
			FillDefaultTargets();
			dataFolderName = ProjectJson.GetValue("DataFolderName", "Data");
			pluginName = ProjectJson.GetValue("Plugin", "");
			citrusLocation = ProjectJson.GetValue("CitrusLocation", string.Empty);

			foreach (var target in ProjectJson.GetArray("Targets", new Dictionary<string, object>[0])) {
				var cleanBeforeBuild = false;
				if (target.ContainsKey("CleanBeforeBuild")) {
					cleanBeforeBuild = (bool)target["CleanBeforeBuild"];
				}

				Targets.Add(new Target(target["Name"] as string, target["Project"] as string,
											 cleanBeforeBuild, GetPlaformByName(target["Platform"] as string)));
			}
		}

		public void SaveCurrentProject()
		{
			ProjectJson.RewriteOrigin();
		}

		public string GetMainBundlePath()
		{
			return GetMainBundlePath(ActivePlatform);
		}

		public string GetMainBundlePath(TargetPlatform platform)
		{
			return Path.ChangeExtension(AssetsDirectory, platform.ToString());
		}

		public string GetBundlePath(string bundleName)
		{
			return GetBundlePath(bundleName, ActivePlatform);
		}

		public string GetBundlePath(string bundleName, TargetPlatform platform)
		{
			if (bundleName == CookingRulesBuilder.MainBundleName) {
				return The.Workspace.GetMainBundlePath(platform);
			} else {
				return Path.Combine(Path.GetDirectoryName(AssetsDirectory), bundleName + GetPlatformSuffix(platform));
			}
		}

		private static TargetPlatform GetPlaformByName(string name)
		{
			try {
				return (TargetPlatform) Enum.Parse(typeof(TargetPlatform), name, true);
			} catch (ArgumentException) {
				throw new Lime.Exception($"Unknown sub-target platform name: {name}");
			}
		}

		private static string GetTangerineCacheBundlePath()
		{
			var name = string
				.Join("_", The.Workspace.ProjectFile.Split(new[] { "\\", "/", ":" }, StringSplitOptions.RemoveEmptyEntries))
				.ToLower(System.Globalization.CultureInfo.InvariantCulture);
			name = Path.ChangeExtension(name, "tancache");
			return Path.Combine(WorkspaceConfig.GetDataPath(), name);
		}
	}
}
