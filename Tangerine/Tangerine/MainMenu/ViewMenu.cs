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

	public class LocalizationMenuFactory
	{
		public static void Rebuild(Menu menu)
		{
			foreach (var item in menu) {
				CommandHandlerList.Global.Disconnect(item);
			}
			menu.Clear();
			foreach (var locale in GetAvailableLocales()) {
				var command = new Command(locale);
				CommandHandlerList.Global.Connect(command, new SetLocaleCommandHandler(locale));
				menu.Add(command);
			}
		}

		private class SetLocaleCommandHandler : CommandHandler
		{
			private readonly string locale;

			public SetLocaleCommandHandler(string locale)
			{
				this.locale = locale;
			}

			public override void Execute()
			{
				Project.Current.SetLocale(locale);
				// Resolution preview applies localization markers either.
				if (Document.Current?.ResolutionPreview.Enabled ?? false) {
					ResolutionPreviewHandler.Execute(Document.Current, true);
				}
			}

			public override void RefreshCommand(ICommand command)
			{
				command.Checked = Project.Locale == locale;
			}
		}

		private static IEnumerable<string> GetAvailableLocales()
		{
			if (Project.Current == Project.Null) {
				yield break;
			}
			// enumerate dictionaries in legacy location (./Data/Dictionary.*.txt) as well as new location (./Data/Localization.*.txt)
			var files = Directory.EnumerateFiles(Project.Current.AssetsDirectory, $"Dictionary*.txt", SearchOption.TopDirectoryOnly)
				.Select(i => Path.GetFileName(i));
			var absoluteDictionariesPath = Path.Combine(Project.Current.AssetsDirectory, Localization.DictionariesPath);
			if (Directory.Exists(absoluteDictionariesPath)) {
				files = files.Union(
					Directory.EnumerateFiles(absoluteDictionariesPath, $"Dictionary*.txt", SearchOption.TopDirectoryOnly)
					.Select(i => Path.GetFileName(i))
				);
			}
			foreach (var file in files.Select(i => Path.GetFileNameWithoutExtension(i))) {
				var locale = Path.GetExtension(file);
				yield return string.IsNullOrEmpty(locale) ? ProjectUserPreferences.DefaultLocale : locale.Substring(1);
			}
		}
	}
}
