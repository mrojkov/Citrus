using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Orange.Source;

namespace Orange
{
	public class SolutionBuilder
	{
		string projectDirectory;
		string projectName;
        string customSolution;
		TargetPlatform platform;

		public static string ConfigurationName = "Release";

		public SolutionBuilder(TargetPlatform platform, string customSolution = null)
		{
			this.platform = platform;
			projectName = The.Workspace.Title;
			projectDirectory = Path.Combine(The.Workspace.ProjectDirectory, projectName);
            this.customSolution = customSolution;
			switch (platform) {
				case TargetPlatform.Android:
					projectDirectory += ".Android";
					break;
				case TargetPlatform.Desktop:
#if WIN
					projectDirectory += ".Win";
#elif MAC || MONOMAC
					projectDirectory += ".Mac";
#endif
					break;
				case TargetPlatform.iOS:
#if WIN
					throw new NotSupportedException();
#elif MAC || MONOMAC
					projectDirectory += ".iOS";
					break;
#endif
				default:
					throw new NotSupportedException();
			}
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
			Subversion.Update(Path.GetDirectoryName(projectDirectory));
		}

		public static void CopyFile(string srcDir, string dstDir, string fileName)
		{
			var srcFile = Path.Combine(srcDir, fileName);
			var dstFile = Path.Combine(dstDir, fileName);
			Console.WriteLine("Copying: {0}", dstFile);
			File.Copy(srcFile, dstFile, true);
		}

		public bool Build(StringBuilder output = null)
		{
			Console.WriteLine("------------- Building Application -------------");
			CsprojSynchronization.SynchronizeAll();
			var buildSystem = GetBuildSystem();
			buildSystem.PrepareForBuild();
			return buildSystem.Execute(output) == 0;
		}

		public bool Clean()
		{
			Console.WriteLine("------------- Cleanup Game Application -------------");
			var buildSystem = GetBuildSystem();
			buildSystem.PrepareForClean();
			return buildSystem.Execute() == 0;
		}

		private BuildSystem GetBuildSystem()
		{
#if WIN
			var buildSystem = new MSBuild(projectDirectory, projectName, platform, customSolution);
#elif MAC
			var buildSystem = new MDTool(projectDirectory, projectName, platform, customSolution);
#else
			throw new NotSupportedException();
#endif
            buildSystem.Configuration = ConfigurationName;
			return buildSystem;
		}



		public string GetApplicationPath()
		{
			string app;
#if MAC
			if (platform == TargetPlatform.Desktop) {
				app = Path.Combine(projectDirectory, string.Format("bin/{0}", ConfigurationName), projectName + ".app", "Contents/MacOS", projectName);
			} else {
				throw new NotImplementedException();
			}
#elif WIN
			app = Path.Combine(projectDirectory, string.Format("bin/{0}", ConfigurationName), projectName + ".exe");
#endif
			return app;
		}

		public int Run(string arguments)
		{
			Console.WriteLine("------------- Starting Application -------------");
#if WIN
			var app = GetApplicationPath();

			if (File.Exists(app)) {
				var dir = Path.GetDirectoryName(app);
				using (new DirectoryChanger(dir)) {
					var exitCode = Process.Start(app, arguments);
					return exitCode;
				}
			} else {
				Console.WriteLine("Error: File not found: " + app);
				return 1;
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
			var directory = Path.Combine(projectDirectory, "bin", "iPhone", ConfigurationName);
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