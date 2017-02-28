using Lime;

namespace EmptyProject.Dialogs
{
	public class AlertDialog : Dialog<Scenes.AlertDialog>
	{
		public AlertDialog(string text)
		{
			var label = Scene._Title.It;
			label.OverflowMode = TextOverflowMode.Minify;
			label.Text = text;
			Scene._BtnOk.It.Clicked = Close;
		}
	}
}
