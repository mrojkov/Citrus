using System;
using System.IO;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class OpenProjectCommand : Command
	{
		public override string Text => "Open Project...";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.OpenProject;

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

	public class OpenCommand : Command
	{
		public override string Text => "Open ...";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.Open;
		public override bool Enabled => Project.Current != null;

		public override void Execute()
		{
			var dlg = new FileDialog {
				AllowedFileTypes = new string[] { Document.SceneFileExtension },
				Mode = FileDialogMode.Open,
			};
			if (Document.Current != null) {
				dlg.InitialDirectory = Path.GetDirectoryName(Document.Current.GetAbsolutePath());
			}
			if (dlg.RunModal()) {
				if (!dlg.FileName.StartsWith(Project.Current.AssetsDirectory)) {
					var alert = new AlertDialog("Tangerine", "Can't open document outside the project assets directory", "Ok");
					alert.Show();
				} else {
					var path = dlg.FileName.Substring(Project.Current.AssetsDirectory.Length + 1);
					Project.Current.OpenDocument(path);
				}
			}
		}
	}

	public class SaveCommand : Command
	{
		public override string Text => "Save";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.Save;
		public override bool Enabled => Document.Current != null;

		public override void Execute()
		{
			Document.Current.Save();
		}
	}

	public class SaveAsCommand : Command
	{
		public override string Text => "Save As...";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.SaveAs;
		public override bool Enabled => Document.Current != null;

		public override void Execute()
		{
			var dlg = new FileDialog {
				AllowedFileTypes = new string[] { Document.SceneFileExtension },
				Mode = FileDialogMode.Save,
				InitialDirectory = Path.GetDirectoryName(Document.Current.GetAbsolutePath())
			};
			if (dlg.RunModal()) {
				Document.Current.SaveAs(dlg.FileName);
			}
		}
	}

	public class CloseDocumentCommand : Command
	{
		public override string Text => "Close Document...";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.CloseDocument;

		public override void Execute()
		{
			if (Document.Current != null) {
				Project.Current.CloseDocument(Document.Current);
			}
		}
	}
}
