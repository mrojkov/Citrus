using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmptyProject.Dialogs
{
	public class SplashScreen : Dialog
	{
		public SplashScreen(Action onClosed = null)
			: base("Shell/Splash")
		{
			Root.Play("Start", () => {
				new ScreenCrossfade(() => {
					CloseImmediately();
					onClosed.SafeInvoke();
				});
			});
		}
	}
}
