﻿#if !iOS
using System;

namespace Lime
{
	public class WebBrowser : Widget, IDisposable
	{
		public Uri Url { get { return null; } set { SetUrl(value); } }

		public WebBrowser()
		{
		}

		public WebBrowser(Widget parentWidget)
		{
			parentWidget.AddNode(this);

		}
		
		private void SetUrl(Uri value)
		{
		}	

		public void Dispose() { }
	}
}
#endif