#if !iOS
using System;

namespace Lime
{
	public class WebBrowser : Widget, IDisposable
	{
		public Uri Url { get { return null; } set { SetUrl(value); } }

		public WebBrowser(Widget parentWidget)
		{
			parentWidget.AddNode(this);
		}
		
		private void SetUrl(Uri value)
		{
			throw new NotImplementedException();
		}	

		public void Dispose() { }
	}
}
#endif