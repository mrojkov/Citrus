using System;
using System.IO;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class FileOpenProject : CommandHandler
	{
		public override void Execute()
		{
			var dlg = new FileDialog { AllowedFileTypes = new string[] { "citproj" }, Mode = FileDialogMode.Open };
			if (dlg.RunModal()) {
				if (Project.Current.Close()) {
					new Project(dlg.FileName).Open();
					var prefs = UserPreferences.Instance;
					prefs.RecentProjects.Remove(dlg.FileName);
					prefs.RecentProjects.Insert(0, dlg.FileName);
					prefs.Save();
				}
			}
		}
	}

	public class FileNew : CommandHandler
	{
		public override void RefreshCommand(ICommand command)
		{
			command.Enabled = Project.Current != null;
		}

		public override void Execute()
		{
			Project.Current.NewDocument();
		}
	}

	public class FileOpen : CommandHandler
	{
		public override void RefreshCommand(ICommand command)
		{
			command.Enabled = Project.Current != null;
		}

		public override void Execute()
		{
			var dlg = new FileDialog {
				AllowedFileTypes = Document.GetSupportedFileTypes(),
				Mode = FileDialogMode.Open,
			};
			if (Document.Current != null) {
				dlg.InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path);
			}
			if (dlg.RunModal()) {
				string localPath;
				if (!Project.Current.TryGetAssetPath(dlg.FileName, out localPath)) {
					var alert = new AlertDialog("Tangerine", "Can't open a document outside the project directory", "Ok");
					alert.Show();
				} else {
					Project.Current.OpenDocument(localPath);
				}
			}
		}
	}

	public class FileSave : DocumentCommandHandler
	{
		static FileSave()
		{
			Document.PathSelector += SelectPath;
		}

		public override void Execute()
		{
			Document.Current.Save();
		}

		public static bool SelectPath(out string path)
		{
			var dlg = new FileDialog {
				AllowedFileTypes = Document.GetSupportedFileTypes(),
				Mode = FileDialogMode.Save,
				InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path)
			};
			path = null;
			if (!dlg.RunModal()) {
				return false;
			}
			if (!Project.Current.TryGetAssetPath(dlg.FileName, out path)) {
				var alert = new AlertDialog("Tangerine", "Can't save the document outside the project directory", "Ok");
				alert.Show();
				return false;
			}
			return true;
		}
	}

	public class FileSaveAs : DocumentCommandHandler
	{
		public override void Execute()
		{
			SaveAs();
		}

		public static void SaveAs()
		{
			var dlg = new FileDialog {
				AllowedFileTypes = Document.GetSupportedFileTypes(),
				Mode = FileDialogMode.Save,
				InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path)
			};
			if (dlg.RunModal()) {
				string assetPath;
				if (!Project.Current.TryGetAssetPath(dlg.FileName, out assetPath)) {
					var alert = new AlertDialog("Tangerine", "Can't save the document outside the project directory", "Ok");
					alert.Show();
				} else {
					Document.Current.SaveAs(assetPath);
				}
			}
		}
	}

	public class FileClose : DocumentCommandHandler
	{
		public override void Execute()
		{
			if (Document.Current != null) {
				Project.Current.CloseDocument(Document.Current);
			}
		}
	}
}
