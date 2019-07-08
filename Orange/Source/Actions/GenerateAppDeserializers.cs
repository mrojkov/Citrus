using System;
using System.IO;
using System.ComponentModel.Composition;

namespace Orange
{
	public static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Generate Yuzu Deserializers For Application Types")]
		public static string GenerateYuzuDeserializersForApp()
		{
			var target = The.UI.GetActiveTarget();

			AssetCooker.CookForPlatform(target);
			if (!BuildGame(target)) {
				return "Can not BuildGame";
			}

			var builder = new SolutionBuilder(
				target.Platform, target.ProjectPath);
			int exitCode = builder.Run("--GenerateYuzuDeserializers");
			if (exitCode != 0) {
				return $"Application terminated with exit code {exitCode}";
			}
			string app = builder.GetApplicationPath();
			string assembly = Path.Combine(Path.GetDirectoryName(app), "Serializer.dll");
			if (!File.Exists(assembly)) {
				Console.WriteLine("{0} doesn't exist", assembly);
				Console.WriteLine(@"Ensure your Application.cs contains following code:
	if (Array.IndexOf(args, ""--GenerateYuzuDeserializers"") >= 0) {
		Lime.Serialization.GenerateDeserializers(""OceanDeserializers.cs"", ""OceanDeserializers"", GetSerializationTypes());
		return;
	}");
				return string.Empty;
			}
			return null;
			// TODO: write location of generated file
		}
	}
}
