using CefSharp;

namespace ChromiumWebBrowser
{
	class LifeSpanHandler: ILifeSpanHandler
	{
		public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, 
			string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, 
			bool userGesture, IWindowInfo windowInfo, ref bool noJavascriptAccess,
			out IWebBrowser newBrowser)
		{
			// Preserve new windows to be opened and load all popup urls in the same browser view
			browserControl.Load(targetUrl);
			newBrowser = browserControl;
			return true;
		}

		public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
		{
			//throw new System.NotImplementedException();
		}

		public bool DoClose(IWebBrowser browserControl, IBrowser browser)
		{
			//throw new System.NotImplementedException();
			return true;
		}

		public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
		{
			//throw new System.NotImplementedException();
		}
	}
}
