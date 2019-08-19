using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Lime;
using Exception = Lime.Exception;

namespace Orange.Source.Actions
{
	public static class InvalidateFonts
	{
		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Invalidate Fonts")]
		public static string InvalidateFontsAction()
		{
			foreach (var configPath in EnumerateFontConfigs(AssetPath.Combine(The.Workspace.AssetsDirectory, "Fonts/"))) {
				Console.WriteLine($"Processing {configPath}..");
				try {
					FontGenerator.UpdateCharSetsAndGenerateFont(configPath, The.Workspace.AssetsDirectory);
				} catch (Exception e) {
					Console.WriteLine($"Failed to generate font using {configPath} config");
					Console.WriteLine(e);
				}
			}
			return null;
		}

		private static IEnumerable<string> EnumerateFontConfigs(string path)
		{
			foreach (var file in Directory.EnumerateFiles(path, $"*.{TftConfig.Extension}")) {
				yield return file;
			}
			foreach (var directory in Directory.GetDirectories(path)) {
				foreach (var config in EnumerateFontConfigs(directory)) {
					yield return config;
				}
			}
		}

	}
}
