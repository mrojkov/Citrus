using System.IO;

#if MAC
using Mono.Unix.Native;
#endif

namespace Orange
{
	internal static class Nuget
	{
		private static readonly string nugetPath;

		static Nuget()
		{
			nugetPath = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "nuget.exe");
#if MAC
			// Mono requiers to set "chmod +x" for assemblies
			var pluxX =
				FilePermissions.S_IRWXU |
				FilePermissions.S_IRGRP |
				FilePermissions.S_IXGRP |
				FilePermissions.S_IROTH |
				FilePermissions.S_IXOTH;

			Syscall.chmod(nugetPath, pluxX);
#endif
		}

		public static int Restore(string projectDirectory)
		{
			return Start($"restore {projectDirectory}");
		}

		public static int Start(string args)
		{
#if WIN
			return Process.Start(nugetPath, args);
#elif MAC
			return Process.Start("mono", $"{nugetPath} {args}");
#endif
		}
	}
}
