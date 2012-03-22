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

		public static string GetDataDirectory(string appName)
		{
			return GetDataDirectory(null, appName, "1.0");
		}

		public static string GetDataDirectory(string companyName, string appName, string appVersion)
		{
#if iOS
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
#else
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
#endif
			if (string.IsNullOrEmpty(companyName)) {
				path = System.IO.Path.Combine(path, appName, appVersion);
			} else {
				path = System.IO.Path.Combine(path, companyName, appName, appVersion);
			}
			System.IO.Directory.CreateDirectory(path);
			return path;
		}

	}
}
