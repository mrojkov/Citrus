using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using Lime;
using Action = System.Action;

namespace Orange
{
	public interface IAtlasPackerMetadata
	{
		string Id { get; }
	}

	public interface IMenuItemMetadata
	{
		[DefaultValue("Unspecified label")]
		string Label { get; }

		[DefaultValue(int.MaxValue)]
		int Priority { get; }
	}

	public class OrangePlugin
	{
		[Import(nameof(Initialize), AllowRecomposition = true, AllowDefault = true)]
		public Action Initialize;

		[Import(nameof(BuildUI), AllowRecomposition = true, AllowDefault = true)]
		public Action<IPluginUIBuilder> BuildUI;

		[Import(nameof(Finalize), AllowRecomposition = true, AllowDefault = true)]
		public Action Finalize;

		[Import(nameof(GetRequiredAssemblies), AllowRecomposition = true, AllowDefault = true)]
		public Func<string[]> GetRequiredAssemblies;

		[ImportMany(nameof(AtlasPackers), AllowRecomposition = true)]
		public IEnumerable<Lazy<Func<string, List<AssetCooker.AtlasItem>, int, int>, IAtlasPackerMetadata>> AtlasPackers { get; set; }

		[ImportMany(nameof(AfterAssetUpdated), AllowRecomposition = true)]
		public IEnumerable<Action<Lime.AssetBundle, CookingRules, string>> AfterAssetUpdated { get; set; }

		[ImportMany(nameof(AfterAssetsCooked), AllowRecomposition = true)]
		public IEnumerable<Action<string>> AfterAssetsCooked { get; set; }

		[Import(nameof(AfterBundlesCooked), AllowRecomposition = true, AllowDefault = true)]
		public Action<IReadOnlyCollection<string>> AfterBundlesCooked;

		[ImportMany(nameof(CommandLineArguments), AllowRecomposition = true)]
		public IEnumerable<Func<string>> CommandLineArguments { get; set; }

		[ImportMany(nameof(MenuItems), AllowRecomposition = true)]
		public IEnumerable<Lazy<Action, IMenuItemMetadata>> MenuItems { get; set; }

		/// <summary>
		/// Used with and as MenuItems but should return null on success or a textual info about error on error
		/// </summary>
		[ImportMany(nameof(MenuItemsWithErrorDetails), AllowRecomposition = true)]
		public IEnumerable<Lazy<Func<string>, IMenuItemMetadata>> MenuItemsWithErrorDetails { get; set; }
	}

	public static class PluginLoader
	{
		public static string CurrentPluginDirectory;
		public static OrangePlugin CurrentPlugin = new OrangePlugin();
		private static CompositionContainer compositionContainer;
		private static readonly AggregateCatalog catalog;
		private static readonly List<ComposablePartCatalog> registeredCatalogs = new List<ComposablePartCatalog>();

		static PluginLoader()
		{
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
			catalog = new AggregateCatalog();
			RegisterAssembly(typeof(PluginLoader).Assembly);
			ResetPlugins();
		}

		private static void ResetPlugins()
		{
			catalog.Catalogs.Clear();
			foreach (var additionalCatalog in registeredCatalogs) {
				catalog.Catalogs.Add(additionalCatalog);
			}
			compositionContainer = new CompositionContainer(catalog);
			try {
				compositionContainer.ComposeParts(CurrentPlugin);
			} catch (CompositionException compositionException) {
				Console.WriteLine(compositionException.ToString());
			}
		}

		public static void RegisterAssembly(Assembly assembly)
		{
			registeredCatalogs.Add(new AssemblyCatalog(assembly));
			ResetPlugins();
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
			CurrentPlugin?.Finalize?.Invoke();
			The.UI.DestroyPluginUI();
			CurrentPlugin = new OrangePlugin();
			ResetPlugins();
			try {
				if (Directory.Exists(CurrentPluginDirectory)) {
					catalog.Catalogs.Add(new DirectoryCatalog(CurrentPluginDirectory));
					ValidateComposition();
				}
			} catch (BadImageFormatException e) {
				Console.WriteLine(e.Message);
			} catch (System.Exception e) {
				Console.WriteLine(e.Message);
			}
			CurrentPlugin?.Initialize?.Invoke();
			var uiBuilder = The.UI.GetPluginUIBuilder();
			if (uiBuilder != null) {
				CurrentPlugin?.BuildUI?.Invoke(uiBuilder);
				The.UI.CreatePluginUI(uiBuilder);
			}
			The.MenuController.CreateAssemblyMenuItems();
		}

		public static void AfterAssetUpdated(Lime.AssetBundle bundle, CookingRules cookingRules, string path)
		{
			foreach (var i in CurrentPlugin.AfterAssetUpdated) {
				i(bundle, cookingRules, path);
			}
		}

		public static void AfterAssetsCooked(string bundleName)
		{
			foreach (var i in CurrentPlugin.AfterAssetsCooked) {
				i(bundleName);
			}
		}

		public static void AfterBundlesCooked(IReadOnlyCollection<string> bundles)
		{
			CurrentPlugin.AfterBundlesCooked?.Invoke(bundles);
		}

		public static string GetCommandLineArguments()
		{
			string result = "";
			if (CurrentPlugin != null) {
				result = GetPluginCommandLineArgumets();
			}
			return result;
		}

		private static string GetPluginCommandLineArgumets()
		{
			return CurrentPlugin.CommandLineArguments.Aggregate("", (current, i) => current + i());
		}

		private static void ValidateComposition()
		{
			var exportedCount = catalog.Parts.SelectMany(p => p.ExportDefinitions).Count();
			var importedCount = 0;

			Func<MemberInfo, bool> isImportMember = (m) =>
				Attribute.IsDefined(m, typeof(ImportAttribute)) ||
				Attribute.IsDefined(m, typeof(ImportManyAttribute));

			foreach (
				var member in typeof(OrangePlugin).GetMembers()
					.Where(m => m is PropertyInfo || m is FieldInfo)
					.Where(m => isImportMember(m))
				) {
				if (member is PropertyInfo) {
					var property = member as PropertyInfo;
					if (property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable))) {
						importedCount += ((ICollection)property.GetValue(CurrentPlugin)).Count;
					} else if (property.GetValue(CurrentPlugin) != null) {
						importedCount++;
					}
				} else if (member is FieldInfo) {
					var field = member as FieldInfo;
					if (field.FieldType.GetInterfaces().Contains(typeof(IEnumerable))) {
						importedCount += ((ICollection)field.GetValue(CurrentPlugin)).Count;
					} else if (field.GetValue(CurrentPlugin) != null ){
						importedCount++;
					}
				}
			}

			if (exportedCount != importedCount) {
				Console.WriteLine(
					$"WARNING: Plugin composition mismatch found.\nThe given assemblies defines [{exportedCount}] " +
					$"exports, but only [{importedCount}] has been imported.\nPlease check export contracts.\n");
			}
		}

		private static readonly Dictionary<string, Assembly> resolvedAssemblies = new Dictionary<string, Assembly>();

		public static IEnumerable<Type> EnumerateTangerineExportedTypes()
		{
			var requiredAssemblies = CurrentPlugin?.GetRequiredAssemblies;
			if (requiredAssemblies == null) {
				yield break;
			}
			foreach (var name in requiredAssemblies()) {
				var aseembly = AssemblyResolve(null, new ResolveEventArgs(name, null));
				foreach (var t in aseembly.GetExportedTypes()) {
					var attr = t.GetCustomAttributes(false).FirstOrDefault(i => i is TangerineExportAttribute);
					if (attr != null) {
						yield return t;
					}
				}
			}
		}

		private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var commaIndex = args.Name.IndexOf(',');
			var name = commaIndex < 0 ? args.Name : args.Name.Substring(0, commaIndex);
			if (string.IsNullOrEmpty(name)) {
				return null;
			}

			var requiredAssemblies = CurrentPlugin?.GetRequiredAssemblies?.Invoke();
			if (requiredAssemblies == null || !requiredAssemblies.Contains(name)) {
				return null;
			}

			Assembly assembly;
			if (!resolvedAssemblies.TryGetValue(name, out assembly)) {
				var dllPath = Path.Combine(CurrentPluginDirectory, name) + ".dll";
				var readAllBytes = File.ReadAllBytes(dllPath);
				assembly = Assembly.Load(readAllBytes);
				resolvedAssemblies.Add(name, assembly);
			}
			return assembly;
		}
	}
}
