using System;
using System.Text;

namespace Orange.Source
{
	class MDTool: BuildSystem
	{
		public MDTool(string projectDirectory, string projectName, TargetPlatform platform, string customSolution = null)
			: base(projectDirectory, projectName, platform, customSolution)
		{
#if MAC
			BuilderPath = "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool";
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
