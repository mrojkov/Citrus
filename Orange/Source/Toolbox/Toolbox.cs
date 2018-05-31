using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Orange
{
	public static class Toolbox
	{
		private static readonly char[] CmdArgumentDelimiters = { ':' };

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
			foreach (var argument in Environment.GetCommandLineArgs()) {
				var parts = argument.Split(CmdArgumentDelimiters, 2);
				if (parts.Length == 2 && parts[0].Equals(name)) {
					return parts[1];
				}
			}
			return null;
		}

		public static bool GetCommandLineFlag(string name)
		{
			return Array.IndexOf(Environment.GetCommandLineArgs(), name) >= 0;
		}

		public static string GetApplicationDirectory()
		{
			var assemblyPath = Assembly.GetExecutingAssembly().GetName().CodeBase;
#if MAC
			if (assemblyPath.StartsWith("file:")) {
				assemblyPath = assemblyPath.Remove(0, 5);
			}
#elif WIN
			if (assemblyPath.StartsWith("file:///")) {
				assemblyPath = assemblyPath.Remove(0, 8);
			}
#endif
			return Path.GetDirectoryName(assemblyPath);
		}

		public static string CalcCitrusDirectory()
		{
			var path = Uri.UnescapeDataString((new Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);
			while (!File.Exists(Path.Combine(path, CitrusVersion.Filename))) {
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
			var fileName = Guid.NewGuid().ToString() + extension;
			return Path.Combine(Path.GetTempPath(), fileName);
		}

		public static string GetRelativePath(string path, string basePath)
		{
			var baseUri = new Uri(Path.GetFullPath(basePath), UriKind.Absolute);
			var uri = new Uri(Path.GetFullPath(path), UriKind.Absolute);
			return baseUri.MakeRelativeUri(uri).OriginalString;
		}
	}
}
