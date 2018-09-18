using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
using Tangerine.UI.Docking;
using Tangerine.UI.SceneView;

namespace Tangerine
{
	public class ViewDefaultLayout : CommandHandler
	{
		public override void Execute()
		{
			DockManager.Instance.ImportState(TangerineApp.Instance.DockManagerInitialState.Clone(), resizeMainWindow: false);
			DockManager.Instance.ResolveAndRefresh();
		}
	}

	public class SaveLayout : CommandHandler
	{
		private static Yuzu.Json.JsonSerializer serializer = new Yuzu.Json.JsonSerializer();

		public override void Execute()
		{
			var dlg = new FileDialog {
				AllowedFileTypes = new string[] { "layout" },
				Mode = FileDialogMode.Save
			};
			if (dlg.RunModal()) {
				string path = dlg.FileName;
				try {
					var state = DockManager.Instance.ExportState();
					using (var fileStream = new FileStream(path, FileMode.OpenOrCreate)) {
						serializer.ToStream(state, fileStream);
					}
				}
				catch (System.Exception e) {
					AlertDialog.Show(e.Message);
				}
			}
		}
	}

	public class LoadLayout : CommandHandler
	{
		private static Yuzu.Json.JsonDeserializer deserializer = new Yuzu.Json.JsonDeserializer();

		public override void Execute()
		{
			var dlg = new FileDialog {
				AllowedFileTypes = new string[] { "layout" },
				Mode = FileDialogMode.Open
			};
			if (dlg.RunModal()) {
				string path = dlg.FileName;
				try {
					using (var fs = new FileStream(path, FileMode.OpenOrCreate)) {
						DockManager.State state = deserializer.FromStream(
							new DockManager.State(), fs
						) as DockManager.State;
						DockManager.Instance.ImportState(state);
						DockManager.Instance.ResolveAndRefresh();
					}
				}
				catch (System.Exception e) {
					AlertDialog.Show(e.Message);
				}
			}
		}
	}

	public class ManageRulers : DocumentCommandHandler
	{
		public override bool GetEnabled()
		{
			return ProjectUserPreferences.Instance.Rulers.Count > 0;
		}

		public override void ExecuteTransaction()
		{
			new ManageRulersDialog();
		}
	}

	public class ProjectLocalization : CommandHandler
	{
		private readonly Locale locale;

		public static Locale Current { get; private set; }

		public ProjectLocalization(Locale locale)
		{
			this.locale = locale;
		}

		public override void Execute()
		{
			Drop(invalidateDisplayedText: false);
			try {
				using (var stream = new FileStream(locale.DictionaryPath, FileMode.Open)) {
					Localization.Dictionary.ReadFromStream(stream);
				}
				Current = locale;
				System.Console.WriteLine($"Localization was successfully loaded from \"{locale.DictionaryPath}\"");
			} catch (System.Exception exception) {
				System.Console.WriteLine($"Can not read localization from \"{locale.DictionaryPath}\": {exception.Message}");
			}
			InvalidateDisplayedText();
		}

		public override void RefreshCommand(ICommand command)
		{
			command.Checked = Current != null && Current.Code == locale.Code;
		}

		public static void Drop(bool invalidateDisplayedText = true)
		{
			Current = null;
			Localization.Dictionary.Clear();
			if (invalidateDisplayedText) {
				InvalidateDisplayedText();
			}
		}

		private static void InvalidateDisplayedText()
		{
			var documents = Project.Current?.Documents;
			if (documents == null) {
				return;
			}

			foreach (var document in documents) {
				foreach (var text in document.RootNode.Descendants.OfType<IText>()) {
					text.Invalidate();
				}
			}
		}

		public static List<Locale> GetLocales()
		{
			var directory = Project.Current?.AssetsDirectory;
			if (string.IsNullOrEmpty(directory)) {
				return null;
			}

			var locales = new List<Locale>();
			const string LocalizationFilesPrefix = "Dictionary";
			const string LocalizationFilesExtension = ".txt";
			var localizationFilesPattern = $"{LocalizationFilesPrefix}*{LocalizationFilesExtension}";
			var localizationFiles = Directory.EnumerateFiles(directory, localizationFilesPattern, SearchOption.TopDirectoryOnly);
			foreach (var file in localizationFiles) {
				var fileName = Path.GetFileNameWithoutExtension(file);
				var locale = fileName?.Substring(LocalizationFilesPrefix.Length) ?? string.Empty;
				if (string.IsNullOrEmpty(locale)) {
					locale = "Default";
				}
				locales.Add(new Locale(locale, file));
			}
			return locales;
		}
	}

	public class Locale
	{
		public readonly string Code;
		public readonly string DictionaryPath;

		public Locale(string code, string dictionaryPath)
		{
			Code = code;
			DictionaryPath = dictionaryPath;
		}
	}
}
