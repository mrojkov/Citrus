using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Orange
{
	public static class Toolbox
	{
		public static string GetCommandLineArg(string name)
		{
			var args = System.Environment.GetCommandLineArgs();
			foreach (var arg in args) {
				var x = arg.Split(':');
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
	}
}