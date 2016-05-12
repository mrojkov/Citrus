using System.IO;
using System.Text;

namespace Orange.Source
{
	class MSBuild: BuildSystem
	{
		public MSBuild(string projectDirectory, string projectName, TargetPlatform platform, string customSolution = null) 
			: base(projectDirectory, projectName, platform, customSolution)
		{
			BuilderPath = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "MSBuild.exe");
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
