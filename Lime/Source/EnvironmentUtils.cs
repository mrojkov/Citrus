using System;

namespace Lime
{
	public static class EnvironmentUtils
	{
#if iOS	
		public static void OpenBrowser(string url)
		{
			var nsUrl = new MonoTouch.Foundation.NSUrl(url);
			MonoTouch.UIKit.UIApplication.SharedApplication.OpenUrl(nsUrl);
		}
#endif
	}
}
