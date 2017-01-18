using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Gtk;
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
		[Import(nameof(OrangePlugin.Initialize), AllowRecomposition = true, AllowDefault = true)]
		public Action Initialize;
		[Import(nameof(OrangePlugin.Finalize), AllowRecomposition = true, AllowDefault = true)]
		public Action Finalize;
		[ImportMany(nameof(AtlasPackers), AllowRecomposition = true)]
		public IEnumerable<Lazy<Func<string, List<AssetCooker.AtlasItem>, int, int>, IAtlasPackerMetadata>> AtlasPackers { get; set; }
		[ImportMany(nameof(AfterAssetsCooked), AllowRecomposition = true)]
		public IEnumerable<Action<string>> AfterAssetsCooked { get; set; }
		[ImportMany(nameof(CommandLineArguments), AllowRecomposition = true)]
		public IEnumerable<Func<string>> CommandLineArguments { get; set; }
		[ImportMany(nameof(MenuItems), AllowRecomposition = true)]
		public IEnumerable<Lazy<Action, IMenuItemMetadata>> MenuItems { get; set; }
	}

	static class PluginLoader
	{
		public static string CurrentPluginDirectory;
		public static OrangePlugin CurrentPlugin = new OrangePlugin();
		private static CompositionContainer compositionContainer;
		private static readonly AggregateCatalog catalog;

		static PluginLoader()
		{
			catalog = new AggregateCatalog();
			ResetPlugins();
		}

		private static void ResetPlugins()
		{
			catalog.Catalogs.Add(new AssemblyCatalog(typeof(PluginLoader).Assembly));
			compositionContainer = new CompositionContainer(catalog);
			try {
				compositionContainer.ComposeParts(CurrentPlugin);
			} catch (CompositionException compositionException) {
				Console.WriteLine(compositionException.ToString());
			}
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
			CurrentPlugin = new OrangePlugin();
			catalog.Catalogs.Clear();
			ResetPlugins();
			try {
				catalog.Catalogs.Add(new DirectoryCatalog(CurrentPluginDirectory));
			} catch (BadImageFormatException e) {
				Console.WriteLine(e.Message);
			} catch (System.Exception e) {
				Console.WriteLine(e.Message);
			}
			CurrentPlugin?.Initialize?.Invoke();
			The.MenuController.CreateAssemblyMenuItems();
		}

		public static void AfterAssetsCooked(string bundleName)
		{
			foreach (var i in CurrentPlugin.AfterAssetsCooked) {
				i(bundleName);
			}
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
	}
}
