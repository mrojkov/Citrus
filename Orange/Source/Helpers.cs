using System;
using System.Collections.Generic;
using System.IO;

namespace Orange
{
	public static class Helpers
	{
		public static string GetTargetPlatformString(TargetPlatform platform)
		{
			switch(platform)
			{
			case TargetPlatform.Desktop:
				return "Desktop";
			case TargetPlatform.iOS:
				return "iOS";
			default:
				throw new Lime.Exception("Invalid target platform");
			}
		}
		
		public static void CreateDirectoryRecursive(string path)
		{
			if (string.IsNullOrEmpty(path))
				return;
			string basePath = Path.GetDirectoryName(path);
			if (basePath != "" && !Directory.Exists(basePath)) {
				CreateDirectoryRecursive(basePath);
			}
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
		}
		
		public static string GetApplicationDirectory()
		{
			string appPath;
			appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().
				GetName().CodeBase);
#if MAC
			if (appPath.StartsWith("file:")) {
				appPath = appPath.Remove(0, 5);
			}
#elif WIN
			if (appPath.StartsWith("file:\\")) {
				appPath = appPath.Remove(0, 6);
			}
#endif
			return appPath;
		}
		
		public static bool IsPathHidden(string path)
		{
			if (path == ".") {
				// "." directory is always hidden.
				path = System.IO.Directory.GetCurrentDirectory();
			}
			return (System.IO.File.GetAttributes(path) & FileAttributes.Hidden) != 0;
		}
		
		public static List<string> GetAllFiles(string directory, string mask, bool removePath)
		{
			List<string> result = new List<string>();
			string[] files = Directory.GetFiles(directory, mask, SearchOption.AllDirectories);
			string skipPath = "";
			foreach (string path in files) {
				string dir = System.IO.Path.GetDirectoryName(path);
				if (IsPathHidden(dir)) {
					skipPath = dir;
					continue;
				}
				if (skipPath != "" && path.StartsWith(skipPath)) {
					continue;
				}
				if (!IsPathHidden(path)) {
					var path1 = path;
					if (removePath) {
						path1 = path.Remove(0, directory.Length + 1);
					}
					result.Add(path1);
				}
			}
			return result;
		}

		public static List<string> GetAllDirectories(string directory, string mask, bool removePath)
		{
			List<string> result = new List<string>();
			string[] directories = Directory.GetDirectories(directory, mask, SearchOption.AllDirectories);
			string skipPath = "";
			foreach (string path in directories) {
				if (IsPathHidden(path)) {
					skipPath = path;
					continue;
				}
				if (skipPath != "" && path.StartsWith(skipPath)) {
					continue;
				}
				var path1 = path;
				if (removePath) {
					path1 = path.Remove(0, directory.Length + 1);
				}
				result.Add(path1);
			}
			return result;
		}

	}
}