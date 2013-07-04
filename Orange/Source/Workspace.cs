using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orange
{
	public class Workspace
	{
		public string ProjectFile { get; private set; }
		public string ProjectDirectory { get; private set; }
		public string AssetsDirectory { get; private set; }
		public string Title { get; private set; }
		public FileEnumerator AssetFiles { get; private set; }

		public static readonly Workspace Instance = new Workspace();

		public TargetPlatform ActivePlatform {
			get { return The.MainWindow.ActivePlatform; }
		}

		public void Load()
		{
			var config = WorkspaceConfig.Load();
			Open(config.CitrusProject);
			The.MainWindow.PlatformPicker.Active = config.TargetPlatform;
			// ActionPicker.Active = config.Action;
		}

		public void Save()
		{
			var config = WorkspaceConfig.Load();
			config.CitrusProject = ProjectFile;
			config.TargetPlatform = (int)ActivePlatform;
			// config.Action = ActionPicker.Active;
			WorkspaceConfig.Save(config);
		}

		public void Open(string file)
		{
			The.MainWindow.ClearLog();
			ProjectFile = file;
			Title = File.ReadAllText(file);
			ProjectDirectory = Path.GetDirectoryName(file);
			AssetsDirectory = Path.Combine(ProjectDirectory, "Data");
			if (!Directory.Exists(AssetsDirectory)) {
				throw new Lime.Exception("Assets folder '{0}' doesn't exist", AssetsDirectory);
			}
			AssetFiles = new FileEnumerator(AssetsDirectory);
			PluginLoader.ScanForPlugins(file);
			The.MainWindow.CitrusProjectChooser.SelectFilename(file);
		}

		public string GetActivePlatformString()
		{
			return Helpers.GetTargetPlatformString(ActivePlatform);
		}

		public string GetBundlePath(TargetPlatform platform)
		{
			return Path.ChangeExtension(AssetsDirectory, Helpers.GetTargetPlatformString(platform));
		}

		public string GetBundlePath()
		{
			return Path.ChangeExtension(AssetsDirectory, GetActivePlatformString());
		}

		public string GetUnityResourcesDirectory()
		{
			string path = Path.Combine(Path.GetDirectoryName(ProjectDirectory),
				Path.GetFileName(ProjectDirectory) + ".Unity", "Assets", "Resources");
			return path;
		}
	}
}
