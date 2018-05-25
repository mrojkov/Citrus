using System;
using System.IO;
using System.Text;

namespace Orange.Source
{
	class MDTool: BuildSystem
	{
		public MDTool(string projectDirectory, string projectName, TargetPlatform platform, string customSolution = null)
			: base(projectDirectory, projectName, platform, customSolution)
		{
#if MAC
			BuilderPath = "/Applications/Visual Studio.app/Contents/MacOS/vstool";
			if (!File.Exists (BuilderPath)) {
				// WARNING: to fix error:
				// "Error while trying to load the project '': The type initializer for 'Xamarin.Player.Remote.PlayerDeviceManager' threw an exception"
				// disable Extension "Xamarine Live Player" in Visual Studio Extensions list
				// @see https://bugzilla.xamarin.com/show_bug.cgi?id=60151
				BuilderPath = "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool";
				if (!File.Exists (BuilderPath)) {
					throw new System.Exception (@"Please install Visual Studio or Xamarin Studio: https://www.visualstudio.com/ru/downloads/");
				}
			}
#elif WIN
			BuilderPath = @"C:\Program Files(x86)\MonoDevelop\bin\mdtool.exe";
#endif
		}

		public override int Execute(StringBuilder output = null)
		{
			return Process.Start(BuilderPath, string.Format("build \"{0}\" {1}", SlnPath, Args), output: output);
		}

		protected override void DecorateBuild()
		{
			Args += " -t:Build";
		}

		protected override void DecorateClean()
		{
			Args += " -t:Clean";
		}

		protected override void DecorateConfiguration()
		{
			string platformSpecification;
			switch (Platform) {
				case TargetPlatform.iOS: {
					platformSpecification = "|iPhone";
					break;
				}
				case TargetPlatform.Win:
				case TargetPlatform.Mac: {
					// Need to research strange behaviour due to this string
					//platformSpecification = "|x86";
					platformSpecification = "";
					break;
				}
			case TargetPlatform.Android: {
					platformSpecification = "";
					break;
				}
				default: {
					throw new NotSupportedException();
				}
			}
			Args += string.Format(" -c:\"{0}{1}\"", Configuration, platformSpecification);
		}
	}
}
