#if WIN
namespace Lime
{
	public interface IWebBrowserFactory
	{
		IWebBrowserImplementation CreateWebBrowserImplementation();
	}
}
#endif