using System;
using System.Diagnostics;
using System.IO;

namespace Launcher
{
	internal class Builder: CommonBuilder
	{
		protected override void DecorateBuildProcess (Process process, string solutionPath)
		{
			process.StartInfo.Arguments = $"build \"{solutionPath}\" -t:Build -c:Release|x86";
		}

		protected override string DefaultSolutionPath => Path.Combine(Environment.CurrentDirectory, "Orange.Mac.sln");

		protected override string DefaultExecutablePath => Path.Combine (Environment.CurrentDirectory, @"bin/Mac/Release/Orange.app/Contents/MacOS/Orange");

		protected override string BuilderPath => "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool";
	}
}