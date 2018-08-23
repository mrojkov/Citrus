#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedFileChooserButton : Widget
	{
		private Widget label;
		private Widget button;
		private string fileName;

		public override bool IsNotDecorated() => false;

		public IFileDialog FileDialog { get; private set; }

		public string FileName
		{
			get { return fileName; }
			set { label.Text = fileName = value; }
		}

		public ThemedFileChooserButton()
		{
			FileDialog = new FileDialog();
			Layout = new HBoxLayout();
			label = new ThemedSimpleText {
				Id = "Label",
				ForceUncutText = false,
				MinMaxHeight = Theme.Metrics.DefaultButtonSize.Y,
				Padding = Theme.Metrics.ControlsPadding,
				LayoutCell = new LayoutCell { StretchX = float.MaxValue }
			};
			button = new ThemedButton {
				Id = "Button",
				Text = "...",
				MinMaxWidth = 20
			};
			button.Clicked += HandleButtonClick;
			PostPresenter = new ThemedFramePresenter(Theme.Colors.GrayBackground, Theme.Colors.ControlBorder);
			AddNode(label);
			AddNode(button);
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
