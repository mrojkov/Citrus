using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;
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

	public class SetLocalization : CommandHandler
	{
		private readonly ProjectLocalization localization;

		public SetLocalization(ProjectLocalization localization)
		{
			this.localization = localization;
		}

		public override void Execute()
		{
			Project.Current.Localization = localization;
			if (Document.Current?.ResolutionPreview.Enabled ?? false) {
				ResolutionPreviewHandler.Execute(Document.Current, true);
			}
		}

		public override void RefreshCommand(ICommand command)
		{
			command.Checked = Project.Current.Localization != null && Project.Current.Localization.Code == localization.Code;
		}

		public static List<ProjectLocalization> GetLocales()
		{
			var directory = Project.Current.AssetsDirectory;
			if (string.IsNullOrEmpty(directory)) {
				return null;
			}

			var locales = new List<ProjectLocalization>();
			const string LocalizationFilesPrefix = "Dictionary";
			const string LocalizationFilesExtension = ".txt";
			var localizationFilesPattern = $"{LocalizationFilesPrefix}*{LocalizationFilesExtension}";
			var localizationFiles = Directory.EnumerateFiles(directory, localizationFilesPattern, SearchOption.TopDirectoryOnly);
			foreach (var file in localizationFiles) {
				var fileName = Path.GetFileNameWithoutExtension(file);
				var locale = fileName?.Substring(LocalizationFilesPrefix.Length) ?? string.Empty;
				locale = locale.TrimStart('.');
				if (string.IsNullOrEmpty(locale)) {
					locale = "Default";
				}
				locales.Add(new ProjectLocalization(locale, file));
			}
			return locales;
		}
	}
}
