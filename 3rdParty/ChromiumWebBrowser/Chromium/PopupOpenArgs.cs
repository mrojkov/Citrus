using System;

namespace ChromiumWebBrowser
{
	public class PopupOpenArgs : EventArgs
	{
		public bool Show;

		public PopupOpenArgs(bool show)
		{
			Show = show;
		}
	}
}
