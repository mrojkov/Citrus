using System;

namespace ChromiumWebBrowser
{
	public class PopupOpenEventArgs : EventArgs
	{
		public bool Show;

		public PopupOpenEventArgs(bool show)
		{
			Show = show;
		}
	}
}
