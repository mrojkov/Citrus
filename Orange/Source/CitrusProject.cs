using System;
using System.IO;

namespace Orange
{
	public class CitrusProject
	{
		public CitrusProject(string fileName)
		{
			Title = File.ReadAllText(fileName);
			ProjectDirectory = Path.GetDirectoryName(fileName);
			AssetsDirectory = Path.Combine(ProjectDirectory, "Data");
			if (!Directory.Exists(AssetsDirectory)) {
				throw new Lime.Exception("Assets folder '{0}' doesn't exist", AssetsDirectory);
			}
			AssetFiles = new FileEnumerator(AssetsDirectory);
		}

		public readonly string ProjectDirectory;
		public readonly string AssetsDirectory;
		public readonly string Title;
		public FileEnumerator AssetFiles;

		public string GetUnityResourcesDirectory()
		{
			string path = Path.Combine(Path.GetDirectoryName(ProjectDirectory),
				Path.GetFileName(ProjectDirectory) + ".Unity", "Assets", "Resources");
			return path;
		}
	}
}

