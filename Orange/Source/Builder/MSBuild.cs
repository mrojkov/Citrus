using Microsoft.Win32;
using System.IO;
using System.Text;

namespace Orange.Source
{
	class MSBuild: BuildSystem
	{
		public MSBuild(string projectDirectory, string projectName, TargetPlatform platform, string customSolution = null)
			: base(projectDirectory, projectName, platform, customSolution)
		{
			if (!TryGetMSBuildPath(out BuilderPath)) {
				System.Diagnostics.Process.Start(@"https://www.microsoft.com/en-us/download/details.aspx?id=48159");
				throw new System.Exception(@"Please install Microsoft Build Tools 2015: https://www.microsoft.com/en-us/download/details.aspx?id=48159");
			}
		}

		public override int Execute(StringBuilder output = null)
		{
			return Process.Start(BuilderPath, string.Format("\"{0}\" {1}", SlnPath, Args), output: output);
		}

		protected override void DecorateBuild()
		{
			Args += " /verbosity:minimal";
			if (Platform == TargetPlatform.Android)
				Args += " /t:PackageForAndroid /t:SignAndroidPackage";
		}

		protected override void DecorateClean()
		{
			Args += "/t:Clean";
		}

		protected override void DecorateConfiguration()
		{
			Args += " /p:Configuration=" + Configuration;
		}

		private bool TryGetMSBuildPath(out string path)
		{
			var visualStudioRegistryPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7");
			if (visualStudioRegistryPath != null) {
				var vsPath = visualStudioRegistryPath.GetValue("15.0", string.Empty) as string;
				var msBuild15Path = Path.Combine(vsPath, "MSBuild", "15.0", "Bin", "MSBuild.exe");
				if (File.Exists(msBuild15Path)) {
					path = msBuild15Path;
					return true;
				}
			}

			// MSBuild from path obtained with RuntimeEnvironment.GetRuntimeDirectory() is unable to compile C#6.0
			var msBuild14Path = Path.Combine(@"C:\Program Files (x86)\MSBuild\14.0\Bin\", "MSBuild.exe");
			if (File.Exists(msBuild14Path)) {
				path = msBuild14Path;
				return true;
			}

			path = null;
			return false;
		}
	}
}
