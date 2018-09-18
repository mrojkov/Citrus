using System;
using System.IO;

namespace Orange
{
	internal static class Nuget
	{
		private static readonly string nugetPath;
#if MAC
		private static readonly string monoPath;
#endif

		static Nuget()
		{
#if MAC
			nugetPath = Path.Combine(Toolbox.GetApplicationDirectory(), "nuget.exe");
#else
			nugetPath = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "nuget.exe");
#endif
			if (!File.Exists(nugetPath)) {
				nugetPath = Path.Combine(Toolbox.CalcCitrusDirectory(), "Orange", "Toolchain.Win", "nuget.exe");
			}

			if (!File.Exists(nugetPath)) {
				throw new InvalidOperationException($"Can't find nuget.exe.");
			}
#if MAC
			var chmodResult = Process.Start("chmod", $"+x {nugetPath}");
			monoPath = Toolbox.GetMonoPath();
#endif
		}

		public static int Restore(string projectDirectory)
		{
			return Start($"restore \"{projectDirectory}\"");
		}

		public static int Start(string args)
		{
#if WIN
			return Process.Start(nugetPath, args);
#elif MAC
			return Process.Start(monoPath, $"{nugetPath} {args}");
#endif
		}
	}
}
