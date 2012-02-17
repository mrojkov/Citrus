using System;

namespace Lime
{
	public static class Environment
	{
		public static void OpenBrowser(string url)
		{
#if iOS
			var nsUrl = new MonoTouch.Foundation.NSUrl(url);
			MonoTouch.UIKit.UIApplication.SharedApplication.OpenUrl(nsUrl);
#else
			System.Diagnostics.Process.Start(url);
#endif
		}

		public static string GetDataDirectory(string companyName, string appName, string appVersion = "1.0")
		{
#if iOS
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
#else
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
#endif
			path = System.IO.Path.Combine(path, companyName, appName, appVersion);
			System.IO.Directory.CreateDirectory(path);
			return path;
		}

	}
}
