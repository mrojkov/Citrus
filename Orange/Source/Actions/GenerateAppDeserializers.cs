using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Generate Yuzu Deserializers For Application Types")]
		public static void GenerateYuzuDeserializersForApp()
		{
			AssetCooker.CookForActivePlatform();
			if (!BuildGame(Orange.TargetPlatform.Desktop)) {
				return;
			}
			var builder = new SolutionBuilder(TargetPlatform.Desktop);
			int exitCode = builder.Run("--GenerateYuzuDeserializers");
			if (exitCode != 0) {
				Console.WriteLine("Application terminated with exit code {0}", exitCode);
				return;
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
				return;
			}
			// TODO: write location of generated file
		}
	}
}
