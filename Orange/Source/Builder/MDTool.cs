using System;
using System.IO;
using System.Text;

namespace Orange.Source
{
	class MDTool: BuildSystem
	{
		private readonly string builderPath;


		public MDTool(TargetPlatform platform, string solutionPath, string configuration)
			: base(platform, solutionPath, configuration)
		{
#if MAC
			builderPath = "/Applications/Visual Studio.app/Contents/MacOS/vstool";
			if (!File.Exists(builderPath)) {
				// WARNING: to fix error:
				// "Error while trying to load the project '': The type initializer for 'Xamarin.Player.Remote.PlayerDeviceManager' threw an exception"
				// disable Extension "Xamarine Live Player" in Visual Studio Extensions list
				// @see https://bugzilla.xamarin.com/show_bug.cgi?id=60151
				builderPath = "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool";
				if (!File.Exists(builderPath)) {
					throw new System.Exception(@"Please install Visual Studio or Xamarin Studio: https://www.visualstudio.com/ru/downloads/");
				}
			}
#elif WIN
			builderPath = @"C:\Program Files(x86)\MonoDevelop\bin\mdtool.exe";
#endif
		}

		protected override int Execute(StringBuilder output)
		{
			return Process.Start(builderPath, $"build \"{SolutionPath}\" {Args}", output: output);
		}

		protected override void DecorateBuild()
		{
			AddArgument("-t:Build");
		}

		protected override void DecorateClean()
		{
			AddArgument("-t:Clean");
		}

		protected override void DecorateConfiguration()
		{
			string platformSpecification;
			switch (Platform) {
				case TargetPlatform.iOS: {
					platformSpecification = "|iPhone";
					break;
				}
				// Need to research strange behaviour due to this string
				// platformSpecification = "|x86";
				case TargetPlatform.Win:
				case TargetPlatform.Mac:
				case TargetPlatform.Android: {
					platformSpecification = string.Empty;
					break;
				}
				default: {
					throw new NotSupportedException();
				}
			}
			AddArgument($"-c:\"{Configuration}{platformSpecification}\"");
		}
	}
}
