using System;
using System.ComponentModel.Composition;
using System.IO;

namespace Orange
{
	public static class RunTangerineAction
	{
		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Run Tangerine")]
		[ExportMetadata("Priority", 2)]
		public static string RunTangerine()
		{
			const string projectName = "Tangerine";
			var projectDirectory = Path.Combine(Toolbox.CalcCitrusDirectory(), projectName);
			Nuget.Restore(projectDirectory);
#if WIN
			var solutionBuilder = new SolutionBuilder(
				TargetPlatform.Win,
				Path.Combine(projectDirectory, projectName + ".Win.sln"),
				"Release");
#elif MAC
			var solutionBuilder = new SolutionBuilder(
				TargetPlatform.Mac,
				Path.Combine(projectDirectory, projectName + ".Mac.sln"),
				"Debug"); // RELEASE requires code signing, use debug for a while.
#endif
			if (!solutionBuilder.Build()) {
				return "Build system has returned error";
			}

			var p = new System.Diagnostics.Process();
#if WIN
			p.StartInfo.FileName = Path.Combine(projectDirectory, "bin/Release/Tangerine.exe");
#elif MAC
			p.StartInfo.FileName = Path.Combine(
				projectDirectory, "bin/Debug/Tangerine.app/Contents/MacOS/Tangerine");
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.EnvironmentVariables.Clear();
			p.StartInfo.EnvironmentVariables.Add("PATH", "/usr/bin");
#endif
			p.Start();
			return null;
		}
	}
}
