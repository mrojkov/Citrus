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

		public static void GenerateDeserializers(string filename, string rootNamespace, List<Type> types)
		{
			var yjdg = new BinaryDeserializerGenerator(rootNamespace) {
				LineSeparator = "\n",
			};
			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms)) {
				yjdg.GenWriter = sw;
				yjdg.GenerateHeader();
				foreach (var generate in types
					.Select(t => yjdg.GetType()
						.GetMethod("Generate")
						.MakeGenericMethod(t))) {
					generate.Invoke(yjdg, new object[] { });
				}
				yjdg.GenerateFooter();
				sw.Flush();
				ms.WriteTo(new FileStream(filename, FileMode.Create));
			}
		}

		public static void GenerateBinaryDeserializers()
		{
			var jd = new BinaryDeserializerGenerator("GeneratedDeserializersBIN", Serialization.YuzuCommonOptions) {
				LineSeparator = "\n",
			};
			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms)) {
				jd.GenWriter = sw;
				jd.GenerateHeader();

				var types = new List<Type>();
				var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Lime", StringComparison.OrdinalIgnoreCase)).First();
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
					jd.Generate(t);
					Console.WriteLine(t.FullName);
				}

				jd.GenerateFooter();
				sw.Flush();
				ms.WriteTo(new FileStream(Path.Combine(Toolbox.CalcCitrusDirectory(), "Lime", "Source", "GeneratedDeserializersBIN.cs"), FileMode.Create));
			}
		}
	}
}
