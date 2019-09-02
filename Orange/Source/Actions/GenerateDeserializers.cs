using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Lime;
using Yuzu.Binary;
using Yuzu.Metadata;

namespace Orange
{
	static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Generate Lime deserializers")]
		[ExportMetadata("Priority", 5)]
		public static void GenerateLimeDeserializersAction()
		{
			GenerateBinaryDeserializers();
			Console.WriteLine("Done. Please rebuild Orange.");
		}

		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Generate Project Deserializers")]
		[ExportMetadata("Priority", 5)]
		public static void GenerateProjectDeserializersAction()
		{
			GenerateBinaryDeserializersForApp();
			Console.WriteLine("Done.");
		}

		public static void GenerateBinaryDeserializersForApp()
		{
			// Ensure all game dlls are up to date
			// TODO: Require game project to support TangerineRelease and OrangerRelease build configurations
			// BuildGame(The.UI.GetActiveTarget(), BuildConfiguration.Release);
			var generatedDeserializersPath = The.Workspace.ProjectDirectory;
			The.Workspace.ProjectJson.JObject.TryGetValue("GeneratedDeserializerPath", out var token);
			if (token != null) {
				generatedDeserializersPath = Path.Combine(generatedDeserializersPath, token.ToString());
			}
			Generate(Path.Combine(generatedDeserializersPath, "YuzuGeneratedBinaryDeserializer.cs"),
				new BinaryDeserializerGenerator("YuzuGenerated", Serialization.YuzuCommonOptions, $"{The.Workspace.Title}Deserializer", "LimeDeserializer") {
					LineSeparator = "\n",
				},
				GenerateForAssemblies(PluginLoader.EnumerateOrangeAndTangerinePluginAssemblies())
			);
		}

		public static void GenerateBinaryDeserializers()
		{
			var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Lime", StringComparison.OrdinalIgnoreCase)).First();
			Generate(Path.Combine(Toolbox.CalcCitrusDirectory(), "Lime", "Source", "YuzuGeneratedBinaryDeserializer.cs"),
				new BinaryDeserializerGenerator("YuzuGenerated", Serialization.YuzuCommonOptions, "LimeDeserializer") {
					LineSeparator = "\n",
				},
				GenerateForAssemblies(new[] { assembly })
			);
		}

		private static Action<BinaryDeserializerGenerator> GenerateForAssemblies(IEnumerable<Assembly> assemblies)
		{
			return (ybdg) => {
				var types = new List<Type>();
				foreach (var assembly in assemblies) {
					foreach (var t in assembly.GetTypes()) {
						if (t.GetCustomAttribute<YuzuDontGenerateDeserializerAttribute>(false) != null) {
							continue;
						}
						if (t.IsGenericType) {
							if (t == typeof(Keyframe<>) || t == typeof(Animator<>)) {
								foreach (var specializationType in AnimatorRegistry.Instance.EnumerateRegisteredTypes()) {
									if (specializationType.Assembly != assembly && !specializationType.Assembly.FullName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase)) {
										continue;
									}
									var specializedType = t.MakeGenericType(new[] { specializationType });
									types.Add(specializedType);
								}
							} else {
								foreach (var specializationType in t.GetCustomAttributes<YuzuSpecializeWithAttribute>().Select(a => a.Type)) {
									var specializedType = t.MakeGenericType(new[] { specializationType });
									types.Add(specializedType);
								}
							}
						} else {
							var meta = Yuzu.Metadata.Meta.Get(t, Serialization.YuzuCommonOptions);
							if (meta.Items.Count != 0) {
								types.Add(t);
							}
						}
					}
					types.Sort((a, b) => a.FullName.CompareTo(b.FullName));
					foreach (var t in types) {
						ybdg.Generate(t);
						Console.WriteLine(t.FullName);
					}
				}
			};
		}

		private static void Generate(string filename, BinaryDeserializerGenerator ybdg, Action<BinaryDeserializerGenerator> fill)
		{
			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms)) {
				ybdg.GenWriter = sw;
				ybdg.GenerateHeader();
				fill(ybdg);
				ybdg.GenerateFooter();
				sw.Flush();
				ms.WriteTo(new FileStream(filename, FileMode.Create));
			}
		}
	}
}
