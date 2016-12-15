using System.IO;

namespace Orange
{
	internal static class Nuget
	{
		public static int Restore(string projectDirectory)
		{
			var nugetPath = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "nuget");
#if MAC
			Mono.Unix.Native.Syscall.chmod(
				nugetPath, Mono.Unix.Native.FilePermissions.S_IXOTH | Mono.Unix.Native.FilePermissions.S_IXUSR);
#endif

			var nugetArgs = $"restore {projectDirectory}";

			return Process.Start(nugetPath, nugetArgs);
		}
	}
}
