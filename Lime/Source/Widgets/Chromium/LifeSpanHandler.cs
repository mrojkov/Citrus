using CefSharp;

namespace Lime.Chromium
{
	class LifeSpanHandler: ILifeSpanHandler
	{
		public bool OnBeforePopup(IWebBrowser browser, string sourceUrl, string targetUrl, ref int x, ref int y, ref int width,
			ref int height)
		{
			// Preserve new windows to be opened and load all popup urls in the same browser view
			browser.Load(targetUrl);
			return true;
		}

		public virtual void OnBeforeClose(IWebBrowser browser)
		{
			// DO NOTHING
		}
	}
}
