using System.IO;
using System.Text;
using Microsoft.Win32;

namespace Orange.Source
{
	class MSBuildNotFound : System.Exception
	{
		public readonly string DownloadUrl;

		public MSBuildNotFound(string message, string downloadUrl) : base(message)
		{
			DownloadUrl = downloadUrl;
		}
	}

	class MSBuild : BuildSystem
	{
		private readonly string builderPath;


		public MSBuild(TargetPlatform platform, string solutionPath, string configuration)
			: base(platform, solutionPath, configuration)
		{
			if (!TryGetMSBuildPath(out builderPath)) {
				const string MSBuildDownloadUrl = "https://visualstudio.microsoft.com/ru/thank-you-downloading-visual-studio/?sku=BuildTools&rel=15";
				throw new MSBuildNotFound(
					$"Please install Microsoft Build Tools 2015: {MSBuildDownloadUrl}",
					MSBuildDownloadUrl
				);
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

		protected override void DecorateRestore()
		{
			AddArgument("/t:Restore");
		}

		protected override void DecorateConfiguration()
		{
			AddArgument("/p:Configuration=" + Configuration);
		}

		private static bool TryGetMSBuildPath(out string path)
		{
			return TryGetPureMSBuild15Path(out path) || TryGetVsMSBuild15Path(out path);
		}

		private static bool TryGetPureMSBuild15Path(out string path)
		{
			const string msBuildPath =
				@"C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe";

			if (File.Exists(msBuildPath)) {
				path = msBuildPath;
				return true;
			}
			path = null;
			return false;
		}

		private static bool TryGetVsMSBuild15Path(out string path)
		{
			path = null;
			var visualStudioRegistryPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7");
			if (visualStudioRegistryPath == null) {
				return false;
			}
			var vsPath = visualStudioRegistryPath.GetValue("15.0", string.Empty) as string;
			var msBuild15Path = Path.Combine(vsPath, "MSBuild", "15.0", "Bin", "MSBuild.exe");
			if (File.Exists(msBuild15Path)) {
				path = msBuild15Path;
				return true;
			}
			return false;
		}
	}
}
