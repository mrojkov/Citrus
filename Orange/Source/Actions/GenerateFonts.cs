using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Calamansi;
using Lime;
using Exception = Lime.Exception;

namespace Orange.Source.Actions
{
	public static class GenerateFonts
	{
		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Invalidate Fonts")]
		public static string GenerateFontsAction()
		{
			foreach (var configPath in EnumerateFontConfigs(AssetPath.Combine(The.Workspace.AssetsDirectory, "Fonts/"))) {
				Console.WriteLine($"Processing {configPath}..");
				try {
					var config = Lime.Yuzu.Instance.Value.ReadObjectFromFile<CalamansiConfig>(AssetPath.Combine(The.Workspace.AssetsDirectory, configPath));
					Calamansi.Calamansi.UpdateMainCharset(config, The.Workspace.AssetsDirectory);
					config.SaveTo(AssetPath.Combine(The.Workspace.AssetsDirectory, configPath));
					var font = new CalamansiFont(config, The.Workspace.AssetsDirectory);
					font.SaveAsTft(Path.ChangeExtension(configPath, null), The.Workspace.AssetsDirectory);
				} catch (Exception e) {
					Console.WriteLine($"Failed to generate font using {configPath} config");
					Console.WriteLine(e);
				}
			}
			return null;
		}

		private static IEnumerable<string> EnumerateFontConfigs(string path)
		{
			foreach (var file in Directory.EnumerateFiles(path, "*.cfc")) {
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
