using System.IO;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class DocumentTabContextMenu
	{
		public static void Create(Document doc)
		{
			var close = new Command("Close", new Close(doc).Execute);
			var save = new Command("Save", new Save(doc).Execute);
			var closeAllButThis = new Command("Close All But This", new CloseAllButThis(doc).Execute);
			var menu = new Menu {
				save,
				close,
				GenericCommands.CloseAll,
				closeAllButThis,
				FilesystemCommands.NavigateTo,
				FilesystemCommands.OpenInSystemFileManager,
			};
			var path = System.IO.Path.Combine(Project.Current.AssetsDirectory, doc.Path);
			FilesystemCommands.NavigateTo.UserData = path;
			FilesystemCommands.OpenInSystemFileManager.UserData = path;
			menu.Popup();
		}

		private class Save : DocumentCommandHandler
		{
			private Document doc;

			public Save(Document doc)
			{
				this.doc = doc;
			}

			static Save()
			{
				Document.PathSelector += SelectPath;
			}

			public override void ExecuteTransaction()
			{
				if (doc.IsModified) {
					try {
						doc?.Save();
					}
					catch (System.Exception e) {
						ShowErrorMessageBox(e);
					}
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
					InitialDirectory = Path.GetDirectoryName(Document.Current.FullPath)
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

		private class CloseAllButThis : DocumentCommandHandler
		{
			private Document doc;

			public CloseAllButThis(Document doc)
			{
				this.doc = doc;
			}

			public override void ExecuteTransaction()
			{
				if (Project.Current.Documents.Count != 0) {
					Project.Current.CloseAllDocumentsButThis(doc);
				}
			}
		}

		private class Close : DocumentCommandHandler
		{
			private Document doc;

			public Close(Document doc)
			{
				this.doc = doc;
			}

			public override void ExecuteTransaction()
			{
				if (doc != null) {
					Project.Current.CloseDocument(doc);
				}
			}
		}
	}
}
