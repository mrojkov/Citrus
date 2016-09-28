using System.IO;
using System.Text;

namespace Orange.Source
{
	class MSBuild: BuildSystem
	{
		public MSBuild(string projectDirectory, string projectName, TargetPlatform platform, string customSolution = null)
			: base(projectDirectory, projectName, platform, customSolution)
		{
			// MSBuild from path obtained with RuntimeEnvironment.GetRuntimeDirectory() is unable to compile C#6.0
			BuilderPath = Path.Combine(@"C:\Program Files (x86)\MSBuild\14.0\Bin\", "MSBuild.exe");
			if (!File.Exists(BuilderPath)) {
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
	}
}
