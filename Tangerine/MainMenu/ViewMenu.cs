using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
using Tangerine.UI.Docking;

namespace Tangerine
{
	public class ViewDefaultLayout : CommandHandler
	{
		public override void Execute()
		{
			DockManager.Instance.ImportState(TangerineApp.Instance.DockManagerInitialState.Clone(), resizeMainWindow: false);
			DockManager.Instance.Refresh();
		}
	}

	public class ManageRulers : DocumentCommandHandler
	{
		public override bool GetEnabled()
		{
			return ProjectUserPreferences.Instance.Rulers.Count > 0;
		}

		public override void Execute()
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
