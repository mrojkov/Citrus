using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Orange
{
	public static class Toolbox
	{
		private static string monoPath;

		public static string ToWindowsSlashes(string path)
		{
			return path.Replace('/', '\\');
		}

		public static string ToUnixSlashes(string path)
		{
			return path.Replace('\\', '/');
		}

		public static string GetCommandLineArg(string name)
		{
			var args = System.Environment.GetCommandLineArgs();
			foreach (var arg in args) {
				var x = arg.Split(new[] {':'}, 2);
				if (x.Length == 2 && x[0] == name) {
					return x[1];
				}
			}
			return null;
		}

		public static bool GetCommandLineFlag(string name)
		{
			var args = System.Environment.GetCommandLineArgs();
			return Array.IndexOf(args, name) >= 0;
		}

		public static string GetApplicationDirectory()
		{
			var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
#if MAC
			if (assemblyPath.StartsWith("file:")) {
				assemblyPath = assemblyPath.Remove(0, 5);
			}
#elif WIN
			if (assemblyPath.StartsWith("file:///")) {
				assemblyPath = assemblyPath.Remove(0, 8);
			}
#endif
			var dir = System.IO.Path.GetDirectoryName(assemblyPath);
			return dir;
		}

		public static string CalcCitrusDirectory()
		{
			var path = Uri.UnescapeDataString((new Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);
			while (!string.Equals(Path.GetFileName(path), "Citrus", StringComparison.CurrentCultureIgnoreCase)) {
				path = Path.GetDirectoryName(path);
				if (string.IsNullOrEmpty(path)) {
					throw new InvalidOperationException("Can't find Orange directory.");
				}
			}
			return path;
		}

		public static string GetMonoPath()
		{
			return "/Library/Frameworks/Mono.framework/Versions/Current/bin/mono";
		}

		public static string GetTargetPlatformString(TargetPlatform platform)
		{
			switch(platform)
			{
			case TargetPlatform.Win:
				return "Win";
			case TargetPlatform.Mac:
				return "Mac";
			case TargetPlatform.iOS:
				return "iOS";
			case TargetPlatform.Android:
				return "Android";
			case TargetPlatform.Unity:
				return "Unity";
			default:
				throw new InvalidOperationException("Invalid target platform");
			}
		}

		public static IEnumerable<MethodInfo> GetAllMethodsWithAttribute(this Assembly assembly, Type attributeType)
		{
			foreach (var type in assembly.GetTypes()) {
				var allMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
				foreach (var method in allMethods
					.Where(m => m.GetCustomAttributes(attributeType, false).Length > 0)
					.ToArray()) {
					yield return method;
				}
			}
		}

		public static string GetTempFilePathWithExtension(string extension)
		{
			var path = Path.GetTempPath();
			var fileName = Guid.NewGuid().ToString() + extension;
			return Path.Combine(path, fileName);
		}

		public static string GetRelativePath(string path, string basePath)
		{
			var baseUri = new Uri(Path.GetFullPath(basePath), UriKind.Absolute);
			var uri = new Uri(Path.GetFullPath(path), UriKind.Absolute);
			return baseUri.MakeRelativeUri(uri).OriginalString;
		}
	}
}