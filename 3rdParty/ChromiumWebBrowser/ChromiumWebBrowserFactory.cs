using Lime;

namespace ChromiumWebBrowser
{
	public class ChromiumWebBrowserFactory: IWebBrowserFactory
	{

		public IWebBrowserImplementation CreateWebBrowserImplementation()
		{
			return new ChromiumWebBrowser();
		}
	}
}
