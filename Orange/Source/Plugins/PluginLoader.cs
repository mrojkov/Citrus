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
using System.Text.RegularExpressions;
using Lime;
using Action = System.Action;
using Exception = System.Exception;

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
		private static readonly Regex ignoredAssemblies = new Regex(
			"^(Lime|System.*|mscorlib.*|Microsoft.*)",
			RegexOptions.Compiled
		);
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
			var pluginConfiguration = BuildConfiguration.Debug;
#else
			var pluginConfiguration = BuildConfiguration.Release;
#endif
			CurrentPluginDirectory = Path.Combine(pluginRoot, "bin", pluginConfiguration);
			CurrentPlugin?.Finalize?.Invoke();
			The.UI.DestroyPluginUI();
			CurrentPlugin = new OrangePlugin();
			ResetPlugins();
			try {
				if (Directory.Exists(CurrentPluginDirectory)) {
					var orangePluginAssemblies = The.Workspace.ProjectJson.GetArray<string>("OrangePluginAssemblies");
					if (orangePluginAssemblies == null) {
						var msg = "Warning: Field 'OrangePluginAssemblies' not found in " + citrusProjectFile;
						The.UI.ShowError(msg);
						Console.WriteLine(msg);
					} else if (orangePluginAssemblies.Length == 0) {
						Console.WriteLine("Warning: Field 'OrangePluginAssemblies' in " + citrusProjectFile + " is empty");
					} else {
						foreach (var path in The.Workspace.ProjectJson.GetArray<string>("OrangePluginAssemblies")) {
							if (!path.Contains("$CONFIGURATION")) {
								Console.WriteLine(
									"Warning: Using '$CONFIGURATION' instead of 'Debug' or 'Release' in dll path" +
									$" is strictly recommended ($CONFIGURATION line not found in {path}");
							}
							var absPath = Path.Combine(The.Workspace.ProjectDirectory,
								path.Replace("$CONFIGURATION", pluginConfiguration));
							if (!File.Exists(absPath)) {
								var msg = "File not found on attempt to import OrangePluginAssemblies: " + absPath;
								The.UI.ShowError(msg);
								throw new FileNotFoundException(msg);
							}
							catalog.Catalogs.Add(new AssemblyCatalog(Assembly.LoadFrom(absPath)));
						}
					}
					ValidateComposition();
				}
			} catch (BadImageFormatException e) {
				Console.WriteLine(e.Message);
			} catch (System.Exception e) {
				Console.WriteLine(e.Message);
			}
			CurrentPlugin?.Initialize?.Invoke();
			var uiBuilder = The.UI.GetPluginUIBuilder();
			try {
				if (uiBuilder != null) {
					CurrentPlugin?.BuildUI?.Invoke(uiBuilder);
					The.UI.CreatePluginUI(uiBuilder);
				}
			} catch (System.Exception e) {
				Orange.UserInterface.Instance.ShowError($"Failed to build Orange Plugin UI with an error: {e.Message}\n{e.StackTrace}");
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

			foreach (string name in requiredAssemblies()) {
				AssemblyResolve(null, new ResolveEventArgs(name, null));
			}

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				string assemblyName = assembly.GetName().Name;

				if (ignoredAssemblies.IsMatch(assemblyName)) {
					continue;
				}

				Type[] exportedTypes;
				try {
					exportedTypes = assembly.GetExportedTypes();
				} catch (Exception) {
					exportedTypes = null;
				}

				if (exportedTypes == null) {
					continue;
				}

				foreach (var t in exportedTypes) {
					if (t.GetCustomAttributes(false).Any(i =>
						i is TangerineRegisterNodeAttribute || i is TangerineRegisterComponentAttribute)) {
						yield return t;
					}
				}
			}
		}

		private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var commaIndex = args.Name.IndexOf(',');
			var name = commaIndex < 0 ? Path.GetFileName(args.Name) : args.Name.Substring(0, commaIndex);
			if (string.IsNullOrEmpty(name)) {
				return null;
			}

			var requiredAssemblies = CurrentPlugin?.GetRequiredAssemblies?.Invoke();
			string foundPath = requiredAssemblies?.FirstOrDefault(assemblyPath =>
				assemblyPath == name || Path.GetFileName(assemblyPath).Equals(name, StringComparison.InvariantCultureIgnoreCase)
			);

			if (foundPath == null) {
				return null;
			}

			Assembly assembly;

			if (!resolvedAssemblies.TryGetValue(name, out assembly)) {
				string dllPath = Path.Combine(CurrentPluginDirectory, foundPath) + ".dll";

				var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

				var existedAssemblyByPath = domainAssemblies.Where(i => {
					try {
						return string.Equals(
							Path.GetFullPath(i.Location),
							Path.GetFullPath(dllPath),
							StringComparison.CurrentCultureIgnoreCase);
					} catch {
						return false;
					}
				});

				if (existedAssemblyByPath.Any()) {
					assembly = existedAssemblyByPath.First();
					resolvedAssemblies.Add(name, assembly);

					return assembly;
				}

				var existedAssemblyByName = domainAssemblies.Where(i => {
					try {
						return string.Equals(i.GetName().Name, name, StringComparison.CurrentCultureIgnoreCase);
					} catch {
						return false;
					}
				});

				if (existedAssemblyByName.Any()) {
					assembly = existedAssemblyByName.First();
					throw new InvalidOperationException(
						$"WARNING: Assembly {name} with path {assembly.Location} has already loaded in domain." +
						$"\nAssembly {name} with path {dllPath} leads to exception.");
				}

				var readAllDllBytes = File.ReadAllBytes(dllPath);
				byte[] readAllPdbBytes = null;
#if DEBUG
				var pdbPath = Path.Combine(CurrentPluginDirectory, name) + ".pdb";
				if (File.Exists(pdbPath)) {
					readAllPdbBytes = File.ReadAllBytes(pdbPath);
				}
#endif
				assembly = Assembly.Load(readAllDllBytes, readAllPdbBytes);
				resolvedAssemblies.Add(name, assembly);
			}
			return assembly;
		}
	}
}
