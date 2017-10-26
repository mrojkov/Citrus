using System;
using System.IO;
using Lime;
using Environment = System.Environment;

namespace Orange
{
	public class FileChooser : Widget
	{
		private EditBox editor;
		public event Action<string> FileChosenByUser;

		public FileChooser()
		{
			Layout = new HBoxLayout { Spacing = 4 };
			editor = new ThemedEditBox {
				LayoutCell = new LayoutCell(Alignment.Center)
			};
			AddNode(editor);
			var button = new ThemedButton {
				Text = "...",
				MinMaxWidth = 20,
				LayoutCell = new LayoutCell(Alignment.Center)
			};
			AddNode(button);
			editor.Submitted += ChooseFileByUser;
			button.Clicked += ButtonClicked;
		}

		private void ChooseFileByUser(string file)
		{
			ChosenFile = file;
			FileChosenByUser?.Invoke(file);
		}

		private void ButtonClicked()
		{
			var dialog = new FileDialog {
				AllowedFileTypes = new[] { "citproj" },
				Mode = FileDialogMode.Open,
				InitialDirectory = InitialDirectory
			};
			if (dialog.RunModal())
				ChooseFileByUser(dialog.FileName);
		}

		private string InitialDirectory => !string.IsNullOrEmpty(ChosenFile) ?
			Directory.GetParent(ChosenFile).FullName :
			Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);

		public string ChosenFile
		{
			get { return editor.Text; }
			set { editor.Text = value; }
		}
	}
}