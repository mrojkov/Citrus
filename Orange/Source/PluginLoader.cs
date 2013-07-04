using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Orange
{
	static class PluginLoader
	{
		public static Assembly CurrentPlugin;
		public static List<Assembly> LoadedPlugins = new List<Assembly>();

		public static void ScanForPlugins(string citrusProjectFile)
		{
			var pluginRoot = Path.ChangeExtension(citrusProjectFile, ".OrangePlugin");
#if DEBUG
			var pluginConfiguration = "Debug";
#else
			var pluginConfiguration = "Release";
#endif
			var pluginDll = Path.GetFileName(pluginRoot) + ".dll";
			var pluginAssembly = Path.Combine(pluginRoot, "bin", pluginConfiguration, pluginDll);
			if (File.Exists(pluginAssembly)) {
				try {
					LoadPlugin(pluginAssembly);
					Console.WriteLine("Loaded plugin: " + pluginDll);
				} catch (Exception e) {
					ResetCurrentPlugin();
					Console.WriteLine(e.Message);
				}
			} else {
				ResetCurrentPlugin();
			}
		}

		private static void ResetCurrentPlugin()
		{
			CurrentPlugin = null;
			The.MenuController.RefreshMenu();
		}

		private static void LoadPlugin(string pluginAssembly)
		{
			// Load assembly without locking its file
			byte[] readAllBytes = File.ReadAllBytes(pluginAssembly);
			var assembly = Assembly.Load(readAllBytes);
			CurrentPlugin = assembly;
			if (!LoadedPlugins.Contains(assembly)) {
				The.MenuController.CreateAssemblyMenuItems(assembly);
			} else {
				The.MenuController.RefreshMenu();
			}
			LoadedPlugins.Add(assembly);
		}
	}
}
