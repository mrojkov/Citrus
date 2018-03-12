using System;
using Lime;

namespace EmptyProject.Dialogs
{
	public class Confirmation : Dialog<Scenes.Confirmation>
	{
		public event Action OkClicked;

		public Confirmation(string text, bool cancelButtonVisible = true)
		{
			var label = Scene._Title.It;
			label.OverflowMode = TextOverflowMode.Minify;
			label.Text = text;

			var cancel = Scene._BtnCancel.It;
			cancel.Visible = cancelButtonVisible;
			cancel.Clicked = Close;
			Scene._BtnOk.It.Clicked = () => {
				OkClicked?.Invoke();
				Close();
			};
		}

		protected override void Closing()
		{
			OkClicked = null;
		}
	}
}
