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

		private string dataFolderName;
		private string pluginName;

		public Workspace()
		{
			Orange.Updater.CheckForUpdates();
			Targets = new List<Target>();
			FillDefaultTargets();
		}

		public string GetPlatformSuffix(TargetPlatform? platform = null)
		{
			if (platform == null) {
				platform = ActivePlatform;
			}
			return "." + Toolbox.GetTargetPlatformString(platform.Value);
		}

		/// <summary>
		/// Returns solution path. E.g: Zx3.Win/Zx3.Win.sln
		/// </summary>
		public string GetSolutionFilePath()
		{
			var path = Path.Combine(The.Workspace.ProjectDirectory, The.Workspace.Title + GetPlatformSuffix(),
				The.Workspace.Title + GetPlatformSuffix() + ".sln");
			return path;
		}

		/// <summary>
		/// Returns main project path. E.g: Zx3.Win/Zx3.Win.csproj
		/// </summary>
		public string GetMainCsprojFilePath()
		{
			return Path.ChangeExtension(GetSolutionFilePath(), ".csproj");
		}

		/// <summary>
		/// Returns Citrus/Lime project path. It is supposed that Citrus lies beside the game.
		/// </summary>
		public string GetLimeCsprojFilePath(TargetPlatform? platform = null)
		{
			if (platform == null) {
				platform = The.Workspace.ActivePlatform;
			}
			return Path.Combine(Path.GetDirectoryName(ProjectDirectory), "Citrus", "Lime", "Lime" + GetPlatformSuffix(platform) + ".csproj");
		}

		public static readonly Workspace Instance = new Workspace();

		public TargetPlatform ActivePlatform => The.UI.GetActiveTarget().Platform;

		public Target ActiveTarget => The.UI.GetActiveTarget();

		public string CustomSolution => The.UI.GetActiveTarget() == null ? null : The.UI.GetActiveTarget().ProjectPath;

		public bool CleanBeforeBuild => The.UI.GetActiveTarget() != null && The.UI.GetActiveTarget().CleanBeforeBuild;

		public JObject JObject { get; private set; }

		public void Load()
		{
			var config = WorkspaceConfig.Load();
			Open(config.CitrusProject);
			The.UI.LoadFromWorkspaceConfig(config);
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
			JObject = JObject.Parse(File.ReadAllText(file));
			ProjectJson = new Json(JObject, file);
			Title = ProjectJson["Title"] as string;
			Targets = new List<Target>();
			FillDefaultTargets();
			dataFolderName = ProjectJson.GetValue("DataFolderName", "Data");
			pluginName = ProjectJson.GetValue("Plugin", "");

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
			return Path.ChangeExtension(AssetsDirectory, Toolbox.GetTargetPlatformString(platform));
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
				return Path.Combine(Path.GetDirectoryName(AssetsDirectory), bundleName + "." + Toolbox.GetTargetPlatformString(platform));
			}
		}

		public string GetUnityProjectDirectory()
		{
			return Path.Combine(ProjectDirectory, Title + ".Unity");
		}

		private static TargetPlatform GetPlaformByName(string name)
		{
			try {
				return (TargetPlatform)Enum.Parse(typeof(TargetPlatform), name, true);
			}
			catch (ArgumentException) {
				throw new Lime.Exception("Uknown sub target platform name: {0}", name);
			}
		}
	}
}
