using Lime;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Orange
{
	public static class SynchronizeDocumentationAction
	{
		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Synchronize documentation")]
		public static string SynchronizeDocumentation()
		{
			foreach (var type in GetNodesTypes().Union(PluginLoader.EnumerateTangerineExportedTypes())) {
				string pageName = type.Name + "." + type.Name;
				if (!PageExists(pageName)) {
					CreatePage(pageName);
					var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
					foreach (var property in properties) {
						pageName = property.DeclaringType.Name + "." + property.Name;
						if (!PageExists(pageName)) {
							CreatePage(pageName);
						}
					}
				}
			}
			return null;
		}

		private static string GetPagePath(string pageName)
		{
			string path = Path.Combine(
				Toolbox.CalcCitrusDirectory(), "Tangerine", "Tangerine", "Documentation", Path.Combine(pageName.Split('.')));
			return path + ".md";
		}

		public static bool PageExists(string pageName) => File.Exists(GetPagePath(pageName));

		private static IEnumerable<Type> GetNodesTypes()
		{
			var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Lime");
			return assembly == null ? new List<Type>() :
				assembly.GetTypes().Where(t => typeof(Node).IsAssignableFrom(t) && t.IsDefined(typeof(TangerineRegisterNodeAttribute)));
		}

		private static void CreatePage(string pageName)
		{
			string path = GetPagePath(pageName);
			if (!Directory.Exists(Path.GetDirectoryName(path))) {
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			}
			File.WriteAllText(path, $"# {pageName} #\nThis page in not done yet");
			Console.WriteLine("Creating: {0}", pageName);
		}
	}
}
