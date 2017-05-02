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
			nugetPath = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "nuget.exe");
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
