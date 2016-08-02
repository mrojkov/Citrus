using System;
using Lime;

namespace EmptyProject.Dialogs
{
	public class Confirmation : Dialog
	{
		public Confirmation(string text, Action onOk, bool cancelButtonVisible = true)
			: base("Shell/Confirmation")
		{
			var label = Root.Find<RichText>("Title");
			label.OverflowMode = TextOverflowMode.Minify;
			label.Text = text;

			Root["BtnOk"].Clicked = () => {
				Close();
				onOk.SafeInvoke();
			};
			Root["BtnCancel"].Visible = cancelButtonVisible;
			Root["BtnCancel"].Clicked = Close;
		}
	}
}
