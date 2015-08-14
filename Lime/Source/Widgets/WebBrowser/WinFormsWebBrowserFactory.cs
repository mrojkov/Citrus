#if WIN
namespace Lime
{
	class WinFormsWebBrowserFactory: IWebBrowserFactory
	{
		public IWebBrowserImplementation CreateWebBrowserImplementation()
		{
			return new WinFormsWebBrowser();
		}
	}
}
#endif