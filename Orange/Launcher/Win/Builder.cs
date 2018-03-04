using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Launcher
{
	internal class Builder: CommonBuilder
	{
		protected override bool AreRequirementsMet()
		{
			if (BuilderPath != null)
				return true;

			Process.Start(@"https://www.microsoft.com/en-us/download/details.aspx?id=48159");
			SetFailedBuildStatus("Please install Microsoft Build Tools 2015");
			return false;
		}

		protected override void DecorateBuildProcess(Process process, string solutionPath)
		{
			process.StartInfo.Arguments = 
				$"\"{solutionPath}\" /t:Build /p:Configuration=Release /p:Platform=x86 /verbosity:minimal";
			var cp = Encoding.Default.CodePage;
			if (cp == 1251)
				cp = 866;
			process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(cp);
			process.StartInfo.StandardErrorEncoding = Encoding.GetEncoding(cp);
		}

		protected override string DefaultSolutionPath => Path.Combine(Environment.CurrentDirectory, "Orange.Win.sln");

		protected override string DefaultExecutablePath => Path.Combine(Environment.CurrentDirectory, @"bin\Win\Release\Orange.GUI.exe");

		protected override string BuilderPath => GetMSBuildPath();

		private string GetMSBuildPath()
		{
			var msBuild14Path = Path.Combine(@"C:\Program Files (x86)\MSBuild\14.0\Bin\", "MSBuild.exe");
			if (File.Exists(msBuild14Path)) {
				return msBuild14Path;
			}

			var visualStudioRegistryPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7");
			if (visualStudioRegistryPath != null) {
				var vsPath = visualStudioRegistryPath.GetValue("15.0", string.Empty) as string;
				var msBuild15Path = Path.Combine(vsPath, "MSBuild", "15.0", "Bin", "MSBuild.exe");
				if (File.Exists(msBuild15Path)) {
					return msBuild15Path;
				}
			}

			return null;
		}
	}
}
