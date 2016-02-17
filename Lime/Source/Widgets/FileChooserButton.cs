#if !ANDROID && !iOS
using System;

namespace Lime
{
	public class FileChooserButton : Widget
	{
		private Widget label;
		private Widget button;
		private string fileName;

		public readonly IFileDialog FileDialog;
		public string FileName { get { return fileName; } set { fileName = value; RefreshLabel(); } }

		public FileChooserButton()
		{
			FileDialog = new FileDialog();
			Theme.Current.Apply(this);
		}

		protected override void Awake()
		{
			label = this["Label"];
			button = this["Button"];
			button.Clicked += HandleButtonClick;
			RefreshLabel();
		}

		private void RefreshLabel()
		{
			if (label != null) {
				label.Text = FileName;
			}
		}

		private void HandleButtonClick()
		{
			if (FileDialog.RunModal()) {
				FileName = label.Text = FileDialog.FileName;
			}
		}
	}
}
#endif
