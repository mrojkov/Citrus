using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Orange
{
	public static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Generate Yuzu Deserializers For Application Types")]
		public static string GenerateYuzuDeserializersForApp()
		{
			AssetCooker.CookForActivePlatform();
#if WIN
			if (!BuildGame(Orange.TargetPlatform.Win)) {
#elif MAC
			if (!BuildGame(Orange.TargetPlatform.Mac)) {
#endif
				return "Can not BuildGame";
			}
#if WIN
			var builder = new SolutionBuilder(TargetPlatform.Win);
#elif MAC
			var builder = new SolutionBuilder(TargetPlatform.Mac);
#endif
			int exitCode = builder.Run("--GenerateYuzuDeserializers");
			if (exitCode != 0) {
				return $"Application terminated with exit code {exitCode}";
			}
			string app = builder.GetApplicationPath();
			string dir = System.IO.Path.GetDirectoryName(app);
			string assembly = System.IO.Path.Combine(dir, "Serializer.dll");
			if (!System.IO.File.Exists(assembly)) {
				Console.WriteLine("{0} doesn't exist", assembly);
				Console.WriteLine(@"Ensure your Application.cs contains following code:
	if (Array.IndexOf(args, ""--GenerateYuzuDeserializers"") >= 0) {
		Lime.Serialization.GenerateDeserializers(""OceanDeserializers.cs"", ""OceanDeserializers"", GetSerializationTypes());
		return;
	}");
				return "";
			}
			return null;
			// TODO: write location of generated file
		}
	}
}
