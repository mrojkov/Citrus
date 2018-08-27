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
				Execute(dlg.FileName);
			}
		}

		public static bool Execute(string fileName)
		{
			if (fileName != default && Project.Current.Close() && fileName.Length > 0) {
				FontPool.Instance.Clear(preserveDefaultFont: true);
				new Project(fileName).Open();
				AddRecentProject(fileName);
				return true;
			}
			return false;
		}

		public static void AddRecentProject(string path)
		{
			var prefs = AppUserPreferences.Instance;
			prefs.RecentProjects.Remove(path);
			prefs.RecentProjects.Insert(0, path);
			if (prefs.RecentProjects.Count > AppUserPreferences.RecentProjectsCount) {
				prefs.RecentProjects.RemoveAt(prefs.RecentProjects.Count - 1);
			}
			UserPreferences.Instance.Save();
		}
	}


	public class ProjectNew : CommandHandler
	{
		public override void Execute()
		{
			FontPool.Instance.Clear(preserveDefaultFont: true);
			Orange.NewProject.NewProjectAction(FileOpenProject.Execute);
		}
	}

	public class FileCloseProject : CommandHandler
	{
		public override void Execute()
		{
			Project.Current.Close();
		}

		public override void RefreshCommand(ICommand command)
		{
			command.Enabled = Project.Current != Project.Null;
		}
	}

	public class FileNew : CommandHandler
	{
		private readonly DocumentFormat format;
		private readonly Type rootType;

		public FileNew(DocumentFormat format = DocumentFormat.Scene, Type rootType = null)
		{
			this.format = format;
			this.rootType = rootType;
		}

		public override void RefreshCommand(ICommand command)
		{
			command.Enabled = Project.Current != Project.Null;
		}

		public override void Execute()
		{
			if (Document.Current != null && Document.Current.History.IsTransactionActive) {
				return;
			}
			Project.Current.NewDocument(format, rootType);
		}
	}

	public class FileOpen : CommandHandler
	{
		public override void RefreshCommand(ICommand command)
		{
			command.Enabled = Project.Current != Project.Null;
		}

		public override void Execute()
		{
			var dlg = new FileDialog {
				AllowedFileTypes = Document.AllowedFileTypes,
				Mode = FileDialogMode.Open,
			};
			if (Document.Current != null) {
				dlg.InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path);
			}
			if (dlg.RunModal()) {
				var document = Project.Current.OpenDocument(dlg.FileName, true);
			}
		}
	}

	public class FileSave : DocumentCommandHandler
	{
		static FileSave()
		{
			Document.PathSelector += SelectPath;
		}

		public override void ExecuteTransaction()
		{
			try {
				Document.Current.Save();
			}
			catch (System.Exception e) {
				ShowErrorMessageBox(e);
			}
		}

		public static void ShowErrorMessageBox(System.Exception e)
		{
			AlertDialog.Show($"Save document error: '{e.Message}'.\nYou may have to upgrade the document format.");
		}

		public static bool SelectPath(out string path)
		{
			var dlg = new FileDialog {
				AllowedFileTypes = new string[] { Document.Current.GetFileExtension() },
				Mode = FileDialogMode.Save,
				InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path)
			};
			path = null;
			if (!dlg.RunModal()) {
				return false;
			}
			if (!Project.Current.TryGetAssetPath(dlg.FileName, out path)) {
				AlertDialog.Show("Can't save the document outside the project directory");
				return false;
			}
			return true;
		}
	}

	public class FileSaveAll : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			foreach (var doc in Project.Current.Documents)
				doc.Save();
		}
	}

	public class FileRevert : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			if (new AlertDialog($"Are you sure you want to revert \"{Document.Current.Path}\"?", "Yes", "Cancel").Show() == 0) {
				Project.Current.RevertDocument(Document.Current);
			}
		}
	}

	public class FileSaveAs : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			SaveAs();
		}

		public static void SaveAs()
		{
			var dlg = new FileDialog {
				AllowedFileTypes = new string[] { Document.Current.GetFileExtension() },
				Mode = FileDialogMode.Save,
				InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path),
				InitialFileName = Path.GetFileNameWithoutExtension(Document.Current.Path)
			};
			if (dlg.RunModal()) {
				string assetPath;
				if (!Project.Current.TryGetAssetPath(dlg.FileName, out assetPath)) {
					AlertDialog.Show("Can't save the document outside the project directory");
				} else {
					try {
						Document.Current.SaveAs(assetPath);
					}
					catch (System.Exception e) {
						FileSave.ShowErrorMessageBox(e);
					}
				}
			}
		}
	}

	public class FileClose : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			if (Document.Current != null) {
				Project.Current.CloseDocument(Document.Current);
			}
		}
	}

	public class FileCloseAll : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			if (Project.Current.Documents.Count != 0) {
				Project.Current.CloseAllDocuments();
			}
		}
	}

	public class FileCloseAllButCurrent : DocumentCommandHandler
	{
		public override void ExecuteTransaction()
		{
			if (Project.Current.Documents.Count != 0) {
				Project.Current.CloseAllDocumentsButThis(Document.Current);
			}
		}
	}

	public class UpgradeDocumentFormat : DocumentCommandHandler
	{
		public override bool GetEnabled()
		{
			return Document.Current.Format == DocumentFormat.Scene;
		}

		public override void ExecuteTransaction()
		{
			try {
				AssetBundle.Current.DeleteFile(Path.ChangeExtension(Document.Current.Path, "scene"));
				Document.Current.Format = DocumentFormat.Tan;
				RestoreDefaultAnimationEngine(Document.Current.RootNode);
				new UpsampleAnimationTwice().Execute();
				Document.Current.Save();
			}
			catch (System.Exception e) {
				AlertDialog.Show($"Upgrade document format error: '{e.Message}'");
			}
		}

		private void RestoreDefaultAnimationEngine(Node node)
		{
			node.DefaultAnimation.AnimationEngine = DefaultAnimationEngine.Instance;
			foreach (var child in node.Nodes) {
				RestoreDefaultAnimationEngine(child);
			}
		}
	}
}
