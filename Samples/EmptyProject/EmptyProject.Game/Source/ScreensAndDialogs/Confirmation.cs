using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace EmptyProject
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
			Root["BtnCancel"].Clicked = () => {
				Close();
			};
		}

	}

}
