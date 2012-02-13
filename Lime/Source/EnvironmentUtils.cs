using System;

namespace Lime
{
	public static class EnvironmentUtils
	{
		public static void OpenBrowser(string url)
		{
#if iOS	
			var nsUrl = new MonoTouch.Foundation.NSUrl(url);
			MonoTouch.UIKit.UIApplication.SharedApplication.OpenUrl(nsUrl);
#else
			System.Diagnostics.Process.Start( url );
#endif
		}
	}
}
