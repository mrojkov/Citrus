using System;
using System.Collections.Generic;
using System.IO;
#if iOS
using MonoTouch;
using UIKit;
#elif ANDROID
using Android.Content;
#elif MAC
using AppKit;
#endif

namespace Lime
{
	public static class Environment
	{
		public static void OpenBrowser(string url)
		{
#if iOS
			var nsUrl = new Foundation.NSUrl(url);
			UIKit.UIApplication.SharedApplication.OpenUrl(nsUrl);
#elif ANDROID
			var uri = Android.Net.Uri.Parse(url);
			var intent = new Intent(Intent.ActionView, uri);
			if (intent.ResolveActivity(ActivityDelegate.Instance.ContentView.Context.PackageManager) != null) {
				ActivityDelegate.Instance.Activity.StartActivity(intent);
			}
#else
			System.Diagnostics.Process.Start(url);
#endif
		}

		/// <summary>
		/// Opens the system file manager and selects a file or folder
		/// </summary>
		/// <param name="path">Absolute path to the file or folder</param>
		public static void ShowInFileManager(string path)
		{
			System.Diagnostics.ProcessStartInfo startInfo =
				new System.Diagnostics.ProcessStartInfo();
#if WIN
			startInfo.FileName = "explorer.exe";
			startInfo.Arguments = $"/select, \"{path}\"";
#elif MAC
			string appleScript = 
				"'tell application \"Finder\"\n" +
				"activate\n" +
				$"make new Finder window to (POSIX file \"{path}\")\n" +
				"end tell'";

			startInfo.FileName = "osascript";
			startInfo.Arguments = $"-e {appleScript}";
#else
			throw new System.NotImplementedException();
#endif
			System.Diagnostics.Process.Start(startInfo);
		}

		public static string GetDataDirectory(string appName)
		{
			return GetDataDirectory(null, appName, "1.0");
		}

		public static string GetPathInsideDataDirectory(string appName, string path)
		{
			return Path.Combine(GetDataDirectory(appName), path);
		}

		public static string GetDataDirectory(string companyName, string appName, string appVersion)
		{
#if iOS
			return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
#else
#if MAC
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
#else
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
#endif // !MAC
			path = string.IsNullOrEmpty(companyName)
				? Path.Combine(path, appName, appVersion)
				: Path.Combine(path, companyName, appName, appVersion);
			Directory.CreateDirectory(path);
			return path;
#endif // !iOS
		}

		public static string GetDownloadableContentDirectory(string appName)
		{
			return GetDownloadableContentDirectory(null, appName, "1.0");
		}

		public static string GetDownloadableContentDirectory(string companyName, string appName, string appVersion)
		{
#if iOS
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			path = Path.Combine(Path.GetDirectoryName(path), "Library", "DLC");
#else
			string path = GetDataDirectory(companyName, appName, appVersion);
			path = Path.Combine(path, "DLC");
#endif
			Directory.CreateDirectory(path);
			return path;
		}

		public static Vector2 GetDesktopSize()
		{
#if iOS
			UIScreen screen = UIScreen.MainScreen;
			return new Vector2((float)screen.Bounds.Width, (float)screen.Bounds.Height);
#elif ANDROID
			var s = ActivityDelegate.Instance.GameView.Size;
			return new Vector2(s.Width, s.Height);
#elif MAC || MONOMAC
			var device = Lime.Platform.DisplayDevice.Default;
			return new Vector2(device.Width, device.Height);
#elif WIN
			var device = OpenTK.DisplayDevice.Default;
			return new Vector2(device.Width, device.Height);
#endif
		}
	}
}
