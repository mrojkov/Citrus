using Lime;

namespace EmptyProject.Dialogs
{
	public class AlertDialog : Dialog
	{
		public AlertDialog(string text) : base("Shell/Alert")
		{
			var label = Root.Find<RichText>("Title");
			label.OverflowMode = TextOverflowMode.Minify;
			label.Text = text;
			Root["BtnOk"].Clicked = Close;
		}
	}
}
