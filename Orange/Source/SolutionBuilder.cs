using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Orange
{
	public class SolutionBuilder
	{
		string projectDirectory;
		string projectName;
		TargetPlatform platform;

		public SolutionBuilder(TargetPlatform platform)
		{
			this.platform = platform;
			projectName = The.Workspace.Title;
			projectDirectory = Path.Combine(The.Workspace.ProjectDirectory, projectName);
#if WIN
			projectDirectory += ".Win";
#elif MAC
			if (platform == TargetPlatform.iOS) {
				projectDirectory += ".iOS";
			} else {
				projectDirectory += ".Mac";
			}
#endif
		}

		public SolutionBuilder(TargetPlatform platform, string projectDirectory, string projectName)
		{
			this.platform = platform;
			this.projectDirectory = projectDirectory;
			this.projectName = projectName;
		}

		public void SvnUpdate()
		{
			Subversion.Update(Path.GetDirectoryName(The.Workspace.GetLimeCsprojFilePath()));
			Subversion.Update(projectDirectory);
		}

		public static void CopyFile(string srcDir, string dstDir, string fileName)
		{
			string srcFile = Path.Combine(srcDir, fileName);
			string dstFile = Path.Combine(dstDir, fileName);
			Console.WriteLine("Copying: {0}", dstFile);
			System.IO.File.Copy(srcFile, dstFile, true);
		}

		public bool Build(StringBuilder output = null)
		{
			Console.WriteLine("------------- Building Game -------------");
			CsprojSynchronization.SynchronizeAll();
			string app, args, slnFile;
#if MAC
			app = "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool";
			if (platform == TargetPlatform.iOS) {
				slnFile = Path.Combine(projectDirectory, projectName + ".iOS.sln");
				args = String.Format("build \"{0}\" -t:Build -c:\"Release|iPhone\"", slnFile);
			} else {
				slnFile = Path.Combine(projectDirectory, projectName + ".Mac.sln");
				args = String.Format("build \"{0}\" -t:Build -c:\"Release|x86\"", slnFile);
			}
#elif WIN
			// Uncomment follow block if you would like to use mdtool instead of MSBuild
			/*
			app = @"C:\Program Files(x86)\MonoDevelop\bin\mdtool.exe";
			slnFile = Path.Combine(projectDirectory, projectName + ".Win.sln");
			args = String.Format("build \"{0}\" -t:Build -c:\"Release|x86\"", slnFile);
			*/

			app = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "MSBuild.exe");
			slnFile = Path.Combine(projectDirectory, projectName + ".Win.sln");
			args = String.Format("\"{0}\" /verbosity:minimal /p:Configuration=Release", slnFile);
#endif
			if (Process.Start(app, args, output: output) != 0) {
				return false;
			}
			return true;
		}

		public bool Clean()
		{
			Console.WriteLine("------------- Cleanup Game Application -------------");
			string app, args, slnFile;
#if MAC
			app = "/Applications/MonoDevelop.app/Contents/MacOS/mdtool";
			if (platform == TargetPlatform.iOS) {
				slnFile = Path.Combine(projectDirectory, projectName + ".iOS.sln");
				args = String.Format("build \"{0}\" -t:Clean -c:\"Release|iPhone\"", slnFile);
			} else {
				slnFile = Path.Combine(projectDirectory, projectName + ".Mac.sln");
				args = String.Format("build \"{0}\" -t:Clean -c:\"Release|x86\"", slnFile);
			}
#elif WIN
			// Uncomment follow block if you would like to use mdtool instead of MSBuild
			/*
			app = @"C:\Program Files(x86)\MonoDevelop\bin\mdtool.exe";
			slnFile = Path.Combine(projectDirectory, projectName + ".Win.sln");
			args = String.Format("build \"{0}\" -t:Clean -c:\"Release|x86\"", slnFile);
			*/

			app = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "MSBuild.exe");
			slnFile = Path.Combine(projectDirectory, projectName + ".Win.sln");
			args = String.Format("\"{0}\" /t:Clean /p:Configuration=Release", slnFile);
#endif
			if (Process.Start(app, args) != 0) {
				return false;
			}
			return true;
		}

		public string GetApplicationPath()
		{
			string app;
#if MAC
			if (platform == TargetPlatform.Desktop) {
				app = Path.Combine(projectDirectory, "bin/Release", projectName + ".app", "Contents/MacOS", projectName);
			} else {
				throw new NotImplementedException();
			}
#elif WIN
			app = Path.Combine(projectDirectory, "bin/Release", projectName + ".exe");
#endif
			return app;
		}

		public int Run(string arguments)
		{
			Console.WriteLine("------------- Starting Application -------------");
#if WIN
			string app = GetApplicationPath();
			string dir = Path.GetDirectoryName(app);
			using (new DirectoryChanger(dir)) {
				int exitCode = Process.Start(app, arguments);
				return exitCode;
			}
#else
			var args = "--installdev=" + GetIOSAppName();
			int exitCode = Process.Start("/Developer/MonoTouch/usr/bin/mtouch", args);
			if (exitCode != 0) {
				return exitCode;
			}
			Console.WriteLine("Please start app manually :)");
			//args = "--launchdev=" + GetBundleId();
			//exitCode = Toolbox.StartProcess("/Developer/MonoTouch/usr/bin/mtouch", args);
			return exitCode;
#endif
		}

		private string GetIOSAppName()
		{
			var directory = Path.Combine(projectDirectory, "bin", "iPhone", "Release");
			// var directory = Path.Combine(Path.GetDirectoryName(The.Workspace.GetSolutionFilePath()), "bin", "iPhone", "Release");
			var dirInfo = new System.IO.DirectoryInfo(directory);
			var all = new List<System.IO.DirectoryInfo>(dirInfo.EnumerateDirectories("*.app"));
			all.Sort((a, b) => b.CreationTime.CompareTo(a.CreationTime));
			if (all.Count > 0) {
				var path = Path.Combine(directory, all[0].FullName);
				return path;
			}
			return null;
		}
	}
}

