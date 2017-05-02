using System;
using Lime;

namespace Orange
{
	public class FileChooser : Widget
	{
		private EditBox editor;
		public event Action<string> FileChosen;

		public FileChooser()
		{
			Layout = new HBoxLayout { Spacing = 4 };
			editor = new EditBox {
				LayoutCell = new LayoutCell(Alignment.Center)
			};
			AddNode(editor);
			var button = new Button {
				Text = "...",
				MinMaxWidth = 20,
				Draggable = true,
				LayoutCell = new LayoutCell(Alignment.Center)
			};
			AddNode(button);
			editor.Submitted += ChooseFile;
			button.Clicked += ButtonClicked;
		}

		public void ChooseFile(string file)
		{
			ChosenFile = file;
			editor.Text = file;
			FileChosen?.Invoke(file);
		}

		private void ButtonClicked()
		{
			var dialog = new FileDialog {
				AllowedFileTypes = new[] { "citproj" },
				Mode = FileDialogMode.Open,
				InitialDirectory = "D:\\Dev\\LetsEat" // Directory.GetCurrentDirectory(),
			};
			if (dialog.RunModal())
				ChooseFile(dialog.FileName);
		}

		public string ChosenFile { get; set; }
	}
}