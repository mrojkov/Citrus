#if MAC
using System;

namespace Lime
{
	public class WebBrowser : Image
	{
		public Uri Url { get; set; }

		public WebBrowser() {}

		public WebBrowser(Widget parentWidget): this()
		{
			parentWidget.AddNode(this);
			Size = parentWidget.Size;
		}
	}
}
#endif