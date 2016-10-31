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

	public class NewCommand : Command
	{
		public override string Text => "New";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.New;
		public override bool Enabled => Project.Current != null;

		public override void Execute()
		{
			Project.Current.NewDocument();
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
				string localPath;
				if (!Project.Current.AbsoluteToAssetPath(dlg.FileName, out localPath)) {
					var alert = new AlertDialog("Tangerine", "Can't open a document outside the project directory", "Ok");
					alert.Show();
				} else {
					Project.Current.OpenDocument(localPath);
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
			if (Document.Current.Path == Document.DefaultPath) {
				SaveAsCommand.SaveAs();
			} else {
				Document.Current.Save();
			}
		}
	}

	public class SaveAsCommand : Command
	{
		public override string Text => "Save As...";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.SaveAs;
		public override bool Enabled => Document.Current != null;

		public override void Execute()
		{
			SaveAs();
		}

		public static void SaveAs()
		{
			var dlg = new FileDialog {
				AllowedFileTypes = new string[] { Document.SceneFileExtension },
				Mode = FileDialogMode.Save,
				InitialDirectory = Path.GetDirectoryName(Document.Current.GetAbsolutePath())
			};
			if (dlg.RunModal()) {
				string localPath;
				if (!Project.Current.AbsoluteToAssetPath(dlg.FileName, out localPath)) {
					var alert = new AlertDialog("Tangerine", "Can't save the document outside the project directory", "Ok");
					alert.Show();
				} else {
					Document.Current.SaveAs(localPath);
				}
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
