using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace EmptyProject
{
	public class AlertDialog : Dialog
	{
		public event Action Closed;

		public AlertDialog(string text)
			: base("Shell/Alert")
		{
			var label = Root.Find<RichText>("Title");
			label.OverflowMode = TextOverflowMode.Minify;
			label.Text = text;

			Root["BtnOk"].Clicked = () => {
				Close();
			};
		}

		public override void Close()
		{
			Closed.SafeInvoke();
			base.Close();
		}

	}

}
