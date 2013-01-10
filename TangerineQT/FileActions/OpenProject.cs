using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qyoto;

namespace Tangerine
{
	public class OpenProjectAction : Action
	{
		public static OpenProjectAction Instance = new OpenProjectAction();

		OpenProjectAction()
		{
			Text = "&Open Project...";
		}

		protected override void OnTriggered()
		{
			var dialog = new QFileDialog(The.DefaultQtParent, "Open Citrus Project");
			dialog.fileMode = QFileDialog.FileMode.ExistingFile;
			dialog.SetNameFilter("*.citproj");
			dialog.Show();
			dialog.HideEvent += dialog_HideEvent;
		}

		void dialog_HideEvent(object sender, QEventArgs<QHideEvent> e)
		{
			var dialog = sender as QFileDialog;
			var files = dialog.SelectedFiles();
			if (files != null && files.Count == 1) {
				OpenProject(files[0]);
			}
		}

		public void OpenProject(string file)
		{
			if (Workspace.Close()) {
				RecentProjectsManager.Instance.Add(file);
				Workspace.Open(file);
			}
		}
	}
}
