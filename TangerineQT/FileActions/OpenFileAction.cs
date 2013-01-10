using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qyoto;

namespace Tangerine
{
	public class OpenFileAction : Action
	{
		public static OpenFileAction Instance = new OpenFileAction();

		OpenFileAction()
		{
			Text = "&Open File...";
			Shortcut = new QKeySequence(QKeySequence.StandardKey.Open);
		}

		protected override void OnTriggered()
		{
			var dialog = new QFileDialog(The.DefaultQtParent, "Open file");
			dialog.SetDirectory(System.IO.Directory.GetCurrentDirectory());
			dialog.fileMode = QFileDialog.FileMode.ExistingFile;
			dialog.SetNameFilter("*.tan");
			dialog.Show();
			dialog.HideEvent += dialog_HideEvent;
		}

		void dialog_HideEvent(object sender, QEventArgs<QHideEvent> e)
		{
			var dialog = sender as QFileDialog;
			var files = dialog.SelectedFiles();
			if (files != null && files.Count == 1) {
				System.IO.Directory.SetCurrentDirectory(dialog.Directory.AbsolutePath());
				The.Workspace.OpenDocument(files[0]);
			}
		}
	}
}
