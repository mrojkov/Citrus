using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public FileEnumerator AssetFiles { get; private set; }
		public Json ProjectJson { get; private set; }
		public string Target { get; private set; }

		private string dataFolderName;

		public string GetPlatformSuffix()
		{
			switch (ActivePlatform) {
				case TargetPlatform.Android:
					return ".Android";
				case TargetPlatform.Desktop:
#if WIN
					return ".Win";
#elif MAC || MONOMAC
					return ".Mac";
#endif
				case TargetPlatform.iOS:
#if WIN
					throw new NotSupportedException();
#elif MAC || MONOMAC
					return ".iOS";
#endif
				default:
					throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Returns solution path. E.g: Zx3.Win/Zx3.Win.sln
		/// </summary>
		public string GetSolutionFilePath()
		{
			var path = Path.Combine(The.Workspace.ProjectDirectory, The.Workspace.Title + GetPlatformSuffix(), The.Workspace.Title + GetPlatformSuffix() + ".sln");
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
		/// Enumerate all game projects. E.g: Zx3.Game/Zx3.Game.Win.csproj
		/// </summary>
		public IEnumerable<string> EnumerateGameCsprojFilePaths()
		{
			var dirInfo = new System.IO.DirectoryInfo(ProjectDirectory);
			foreach (var fileInfo in dirInfo.GetFiles("*" + GetPlatformSuffix() + ".csproj", SearchOption.AllDirectories)) {
				var file = fileInfo.FullName;
				yield return file;
			}
		}

		/// <summary>
		/// Returns Citrus/Lime project path. It is supposed that Citrus lies beside the game.
		/// </summary>
		public string GetLimeCsprojFilePath()
		{
			return Path.Combine(Path.GetDirectoryName(ProjectDirectory), "Citrus", "Lime", "Lime" + GetPlatformSuffix() + ".csproj");
		}

		public static readonly Workspace Instance = new Workspace();

		public TargetPlatform ActivePlatform {
			get { return The.UI.GetActivePlatform(); }
		}

		public void Load()
		{
			var config = WorkspaceConfig.Load();
			Open(config.CitrusProject);
//#if MAC
			//The.UI.PlatformPicker.Active = TargetPlatform.iOS;
//#endif
			// The.MainWindow.PlatformPicker.Active = config.TargetPlatform;
			// The.MainWindow.UpdateBeforeBuildCheckbox.Active = config.UpdateBeforeBuild;
			// ActionPicker.Active = config.Action;
		}

		public void Save()
		{
			var config = WorkspaceConfig.Load();
			config.CitrusProject = ProjectFile;
			config.TargetPlatform = (int)ActivePlatform;
			// config.UpdateBeforeBuild = The.MainWindow.UpdateBeforeBuildCheckbox.Active;
			// config.Action = ActionPicker.Active;
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
				PluginLoader.ScanForPlugins(file);
				AssetFiles = new FileEnumerator(AssetsDirectory);
				The.UI.OnWorkspaceOpened();
			} catch (System.Exception e) {
				Console.WriteLine(string.Format("Can't open {0}:\n{1}", file, e.Message));
			}
		}

		private void ReadProject(string file)
		{
			var jobject = JObject.Parse(File.ReadAllText(file));
			ProjectJson = new Json(jobject, file);
			Title = ProjectJson["Title"] as string;
			Target = ProjectJson.GetValue("Target", "") as string;
			dataFolderName = ProjectJson.GetValue("DataFolderName", "Data") as string;
		}

		public string GetActivePlatformString()
		{
			return Toolbox.GetTargetPlatformString(ActivePlatform);
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
			if (bundleName == CookingRules.MainBundleName) {
				return The.Workspace.GetMainBundlePath(platform);
			} else {
				return Path.Combine(Path.GetDirectoryName(AssetsDirectory), bundleName + "." + Toolbox.GetTargetPlatformString(platform));
			}
		}

		public string GetUnityProjectDirectory()
		{
			return Path.Combine(ProjectDirectory, Title + ".Unity");
		}
	}
}
