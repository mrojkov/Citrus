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

		public static void AfterAssetsCooked(string bundleName)
		{
			RunCurrentPluginStaticMethodWithAttribute<PluginAssetsCookedAttribute>(new object[] {bundleName});
		}

		private static void ResetCurrentPlugin()
		{
			CurrentPlugin = null;
			The.UI.RefreshMenu();
		}

		private static void LoadPlugin(string pluginAssembly)
		{
			RunCurrentPluginStaticMethodWithAttribute<PluginFinalizationAttribute>();
			using (new DirectoryChanger(Path.GetDirectoryName(pluginAssembly))) {
				var assembly = LoadAssembly(pluginAssembly);
				CurrentPlugin = assembly;
				RunCurrentPluginStaticMethodWithAttribute<PluginInitializationAttribute>();
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
			if (!File.Exists(assemblyDll)) {
				Console.WriteLine("Warning: missing assembly {0}", Path.GetFileName(assemblyDll));
				return null;
			}
			Console.WriteLine("Loaded assembly: {0}", assemblyDll);
			return Assembly.LoadFrom(assemblyDll);
			// Non blocking version of assembly load
			//byte[] readAllBytes = File.ReadAllBytes(assemblyDll);
			//var assembly = Assembly.Load(readAllBytes);
			//return assembly;
		}

		public static string GetCommandLineArguments()
		{
			string result = "";
			if (CurrentPlugin != null) {
				result = GetPluginCommandLineArgumets(CurrentPlugin);
			}
			return result;
		}

		public static void FilterFiles(List<FileInfo> files)
		{
			if (CurrentPlugin != null) {
				foreach (var method in CurrentPlugin.GetAllMethodsWithAttribute(typeof(PluginFileIgoranceAttribute))) {
					if (!method.IsStatic) {
						new System.Exception(string.Format("'{0}' must be a static method", method.Name));
					}
					var parameters = new object[1] { files };
					method.Invoke(null, parameters);
				}
			}
		}

		private static string GetPluginCommandLineArgumets(System.Reflection.Assembly assembly)
		{
			string result = "";
			foreach (var method in assembly.GetAllMethodsWithAttribute(typeof(PluginCommandLineArgumentsAttribute))) {
				if (!method.IsStatic) {
					new Exception(string.Format("'{0}' must be a static method", method.Name));
				}
				result = result + method.Invoke(null, null) + " ";
			}
			return result;
		}

		private static void RunCurrentPluginStaticMethodWithAttribute<T>(params object[] args) where T : Attribute
		{
			if (CurrentPlugin != null) {
				foreach (var method in CurrentPlugin.GetAllMethodsWithAttribute(typeof(T))) {
					if (!method.IsStatic) {
						throw new Exception(string.Format("'{0}' must be a static method", method.Name));
					}
					var parameters = method.GetParameters();
					if (parameters.Length > args.Length) {
						throw new Exception(string.Format("Wrong number of parameters in '{0}'", method.Name));
					}

					var args2 = args;
					if (parameters.Length < args.Length) {
						args2 = new object[parameters.Length];
						Array.Copy(args, 0, args2, 0, parameters.Length);
					};
					method.Invoke(null, args2);
				}
			}
		}

	}
}
