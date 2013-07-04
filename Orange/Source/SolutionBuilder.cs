using System;
using System.IO;

namespace Orange
{
	public class SolutionBuilder
	{
		TargetPlatform plaform;

		public SolutionBuilder(TargetPlatform plaform)
		{
			this.plaform = plaform;
		}

		public static void CopyFile(string srcDir, string dstDir, string fileName)
		{
			string srcFile = Path.Combine(srcDir, fileName);
			string dstFile = Path.Combine(dstDir, fileName);
			Console.WriteLine("Copying: {0}", dstFile);
			System.IO.File.Copy(srcFile, dstFile, true);
		}

		public bool Build()
		{
			Console.WriteLine("------------- Building Game Application -------------");
			string app, args, slnFile;
#if MAC
			app = "/Applications/MonoDevelop.app/Contents/MacOS/mdtool";
			if (platform == TargetPlatform.iOS) {
				slnFile = Path.Combine(The.Workspace.ProjectDirectory, The.Workspace.Title + ".iOS", The.Workspace.Title + ".iOS.sln");
				args = String.Format("build \"{0}\" -t:Build -c:\"Release|iPhone\"", slnFile);
			} else {
				slnFile = Path.Combine(The.Workspace.ProjectDirectory, The.Workspace.Title + ".Mac", The.Workspace.Title + ".Mac.sln");
				args = String.Format("build \"{0}\" -t:Build -c:\"Release|x86\"", slnFile);
			}
#elif WIN
			// Uncomment follow block if you would like to use mdtool instead of MSBuild
			/*
			app = @"C:\Program Files(x86)\MonoDevelop\bin\mdtool.exe";
			slnFile = Path.Combine(The.Workspace.ProjectDirectory, The.Workspace.Title + ".Win", The.Workspace.Title + ".Win.sln");
			args = String.Format("build \"{0}\" -t:Build -c:\"Release|x86\"", slnFile);
			*/

			app = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "MSBuild.exe");
			slnFile = Path.Combine(The.Workspace.ProjectDirectory, The.Workspace.Title + ".Win", The.Workspace.Title + ".Win.sln");
			args = String.Format("\"{0}\" /verbosity:minimal /p:Configuration=Release", slnFile);
#endif
			if (Helpers.StartProcess(app, args) != 0) {
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
				slnFile = Path.Combine(project.ProjectDirectory, project.Title + ".iOS", project.Title + ".iOS.sln");
				args = String.Format("build \"{0}\" -t:Clean -c:\"Release|iPhone\"", slnFile);
			} else {
				slnFile = Path.Combine(project.ProjectDirectory, project.Title + ".Mac", project.Title + ".Mac.sln");
				args = String.Format("build \"{0}\" -t:Clean -c:\"Release|x86\"", slnFile);
			}
#elif WIN
			// Uncomment follow block if you would like to use mdtool instead of MSBuild
			/*
			app = @"C:\Program Files(x86)\MonoDevelop\bin\mdtool.exe";
			slnFile = Path.Combine(project.ProjectDirectory, project.Title + ".Win", project.Title + ".Win.sln");
			args = String.Format("build \"{0}\" -t:Clean -c:\"Release|x86\"", slnFile);
			*/

			app = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "MSBuild.exe");
			slnFile = Path.Combine(The.Workspace.ProjectDirectory, The.Workspace.Title + ".Win", The.Workspace.Title + ".Win.sln");
			args = String.Format("\"{0}\" /t:Clean /p:Configuration=Release", slnFile);
#endif
			if (Helpers.StartProcess(app, args) != 0) {
				return false;
			}
			return true;
		}

		public string GetApplicationPath()
		{
			string app;
#if MAC
			if (platform == TargetPlatform.Desktop) {
				app = Path.Combine(project.ProjectDirectory, project.Title + ".Mac", "bin/Release", project.Title + ".app", "Contents/MacOS", project.Title);
			} else {
				throw new NotImplementedException();
			}
#elif WIN
			app = Path.Combine(The.Workspace.ProjectDirectory, The.Workspace.Title + ".Win", "bin/Release", The.Workspace.Title + ".exe");
#endif
			return app;
		}

		public int Run(string arguments)
		{
			Console.WriteLine("------------- Starting Application -------------");
			string app = GetApplicationPath();
			string dir = Path.GetDirectoryName(app);
			using (new DirectoryChanger(dir)) {
				int exitCode = Helpers.StartProcess(app, arguments);
				return exitCode;
			}
		}
	}
}

