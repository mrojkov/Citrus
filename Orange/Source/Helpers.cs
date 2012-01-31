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

		public enum StartProcessOptions
		{
			RedirectOutput = 1,
			RedirectErrors = 2
		}

		public static int StartProcess(string app, string args, StartProcessOptions options = StartProcessOptions.RedirectOutput | StartProcessOptions.RedirectErrors)
		{
			var p = new System.Diagnostics.Process();
			p.StartInfo.FileName = app;
			p.StartInfo.Arguments = args;
			p.StartInfo.UseShellExecute = false;
#if WIN
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.WorkingDirectory = Path.GetDirectoryName(app);
			int cp = System.Text.Encoding.Default.CodePage;
			if (cp == 1251)
				cp = 866;
			p.StartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding(cp);
			p.StartInfo.StandardErrorEncoding = System.Text.Encoding.GetEncoding(cp);
#else
			p.StartInfo.StandardOutputEncoding = System.Text.Encoding.Default;
			p.StartInfo.StandardErrorEncoding = System.Text.Encoding.Default;
			p.StartInfo.EnvironmentVariables.Clear();
#endif
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			var logger = new System.Text.StringBuilder();
			if ((options & StartProcessOptions.RedirectOutput) != 0) {
				p.OutputDataReceived += (sender, e) => {
					lock (logger) {
						if (e.Data != null)
							logger.AppendLine(e.Data);
					}
				};
			}
			if ((options & StartProcessOptions.RedirectErrors) != 0) {
				p.ErrorDataReceived += (sender, e) => {
					lock (logger) {
						if (e.Data != null)
							logger.AppendLine(e.Data);
					}
				};
			}
			p.Start();
			p.BeginOutputReadLine();
			p.BeginErrorReadLine();
			while (!p.HasExited) {
				p.WaitForExit(50);
				lock(logger) {
					if (logger.Length > 0) {
						Console.Write(logger.ToString());
						logger.Clear();
					}
				}
				while (Gtk.Application.EventsPending()) {
					Gtk.Application.RunIteration();
				}
			}
			return p.ExitCode;
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