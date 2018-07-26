using System;
using System.IO;
using System.Reflection;

namespace Orange
{
	public static class Toolbox
	{
		public static string GetApplicationDirectory ()
		{
			var assemblyPath = Assembly.GetExecutingAssembly ().GetName ().CodeBase;
#if MAC
			if (assemblyPath.StartsWith("file:")) {
				assemblyPath = assemblyPath.Remove (0, 5);
			}
#elif WIN
			if (assemblyPath.StartsWith("file:///")) {
				assemblyPath = assemblyPath.Remove(0, 8);
			}
#endif
			return Path.GetDirectoryName (assemblyPath);
		}

		public static string CalcCitrusDirectory ()
		{
			var path = Uri.UnescapeDataString ((new Uri (Assembly.GetExecutingAssembly ().CodeBase)).AbsolutePath);
			while (!File.Exists (Path.Combine (path, CitrusVersion.Filename))) {
				path = Path.GetDirectoryName (path);
				if (string.IsNullOrEmpty (path)) {
					throw new InvalidOperationException ("Can't find Orange directory.");
				}
			}
			return path;
		}

		public static string GetMonoPath ()
		{
			return "/Library/Frameworks/Mono.framework/Versions/Current/bin/mono";
		}
	}
}
