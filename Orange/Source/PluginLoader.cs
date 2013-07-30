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
		public static string CurrentPluginDirectory;
		public static Assembly CurrentPlugin;
		public static List<Assembly> LoadedPlugins = new List<Assembly>();

		static PluginLoader()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var name = args.Name.Split(',')[0];
			var path = Path.Combine(CurrentPluginDirectory, name + ".dll");
			return LoadAssembly(path);
		}

		public static void ScanForPlugins(string citrusProjectFile)
		{
			var pluginRoot = Path.ChangeExtension(citrusProjectFile, ".OrangePlugin");
#if DEBUG
			var pluginConfiguration = "Debug";
#else
			var pluginConfiguration = "Release";
#endif
			CurrentPluginDirectory = Path.Combine(pluginRoot, "bin", pluginConfiguration);
			var pluginDll = Path.GetFileName(pluginRoot) + ".dll";
			var pluginAssembly = Path.Combine(CurrentPluginDirectory, pluginDll);
			if (File.Exists(pluginAssembly)) {
				try {
					LoadPlugin(pluginAssembly);
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
			The.UI.RefreshMenu();
		}

		private static void LoadPlugin(string pluginAssembly)
		{
			using (new DirectoryChanger(Path.GetDirectoryName(pluginAssembly))) {
				var assembly = LoadAssembly(pluginAssembly);
				CurrentPlugin = assembly;
				if (!LoadedPlugins.Contains(assembly)) {
					The.MenuController.CreateAssemblyMenuItems(assembly);
				} else {
					The.UI.RefreshMenu();
				}
				LoadedPlugins.Add(assembly);
			}
		}

		private static Assembly LoadAssembly(string assemblyDll)
		{
			Console.WriteLine("Loaded assembly: {0}", Path.GetFileName(assemblyDll));
			byte[] readAllBytes = File.ReadAllBytes(assemblyDll);
			var assembly = Assembly.Load(readAllBytes);
			return assembly;
		}
	}
}
