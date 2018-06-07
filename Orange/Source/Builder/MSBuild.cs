using System.IO;
using System.Text;
using Microsoft.Win32;

namespace Orange.Source
{
	class MSBuild : BuildSystem
	{
		private readonly string builderPath;


		public MSBuild(TargetPlatform platform, string solutionPath, string configuration)
			: base(platform, solutionPath, configuration)
		{
			if (!TryGetMSBuildPath(out builderPath)) {
				System.Diagnostics.Process.Start(@"https://www.microsoft.com/en-us/download/details.aspx?id=48159");
				throw new System.Exception(@"Please install Microsoft Build Tools 2015: https://www.microsoft.com/en-us/download/details.aspx?id=48159");
			}
		}

		protected override int Execute(StringBuilder output)
		{
			return Process.Start($"cmd", $"/C \"set BUILDING_WITH_ORANGE=true & \"{builderPath}\" \"{SolutionPath}\" {Args}\"", output: output);
		}

		protected override void DecorateBuild()
		{
			AddArgument("/verbosity:minimal");
			if (Platform == TargetPlatform.Android) {
				AddArgument("/t:PackageForAndroid");
				AddArgument("/t:SignAndroidPackage");
			}
		}

		protected override void DecorateClean()
		{
			AddArgument("/t:Clean");
		}

		protected override void DecorateConfiguration()
		{
			AddArgument("/p:Configuration=" + Configuration);
		}

		private static bool TryGetMSBuildPath(out string path)
		{
			// MSBuild from path obtained with RuntimeEnvironment.GetRuntimeDirectory() is unable to compile C#6.0
			var msBuild14Path = Path.Combine(@"C:\Program Files (x86)\MSBuild\14.0\Bin\", "MSBuild.exe");
			if (File.Exists(msBuild14Path)) {
				path = msBuild14Path;
				return true;
			}

			var visualStudioRegistryPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7");
			if (visualStudioRegistryPath != null) {
				var vsPath = visualStudioRegistryPath.GetValue("15.0", string.Empty) as string;
				var msBuild15Path = Path.Combine(vsPath, "MSBuild", "15.0", "Bin", "MSBuild.exe");
				if (File.Exists(msBuild15Path)) {
					path = msBuild15Path;
					return true;
				}
			}

			path = null;
			return false;
		}
	}
}
