using System;
using System.IO;

namespace Orange
{
	public class SolutionBuilder
	{
		private CitrusProject project;
		private TargetPlatform platform;
		private string projectName;
		
		public SolutionBuilder (CitrusProject project, TargetPlatform platform)
		{
			this.project = project;
			this.platform = platform;
		}
		
		public static void CopyFile (string srcDir, string dstDir, string fileName)
		{
			string srcFile = Path.Combine (srcDir, fileName);
			string dstFile = Path.Combine (dstDir, fileName);
			Console.WriteLine ("Copying: {0}", dstFile);
			System.IO.File.Copy (srcFile, dstFile, true);
		}

		private int StartProcess (string app, string args)
		{
			var p = new System.Diagnostics.Process ();
			p.StartInfo.FileName = app;
			p.StartInfo.Arguments = args;
			p.StartInfo.UseShellExecute = false;
#if WIN
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.WorkingDirectory = Path.GetDirectoryName (app);
			int cp = System.Text.Encoding.Default.CodePage;
			if (cp == 1251)
				cp = 866;
			p.StartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding (cp);
			p.StartInfo.StandardErrorEncoding = System.Text.Encoding.GetEncoding (cp);
#else
			p.StartInfo.StandardOutputEncoding = System.Text.Encoding.Default;
			p.StartInfo.StandardErrorEncoding = System.Text.Encoding.Default;
			p.StartInfo.EnvironmentVariables.Clear ();
#endif
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			var logger = new System.Text.StringBuilder ();
			p.OutputDataReceived += (sender, e) => {
				lock (logger) {
					if (args == "") {
						string x = e.Data;
					}
					logger.AppendLine (e.Data);
				}
			};
			p.ErrorDataReceived += (sender, e) => {
				lock (logger)
					logger.AppendLine (e.Data);
			};
			p.Start ();
			p.BeginOutputReadLine ();
			p.BeginErrorReadLine ();
			while (!p.HasExited) {
				p.WaitForExit (50);
				lock (logger) {
					if (logger.Length > 0) {
						Console.Write (logger.ToString ());
						logger.Clear ();
					}
				}
				while (Gtk.Application.EventsPending ()) {
					Gtk.Application.RunIteration ();
				}
			}
			return p.ExitCode;
		}
		
		public bool Build ()
		{
			Console.WriteLine ("------------- Building Game Application -------------");
			string app, args, slnFile;
#if MAC
			app = "/Applications/MonoDevelop.app/Contents/MacOS/mdtool";
			if (platform == TargetPlatform.iOS) {
				slnFile = Path.Combine (project.ProjectDirectory, project.Title + ".iOS", project.Title + ".iOS.sln");
				args = String.Format ("build \"{0}\" -t:Build -c:\"Release|iPhone\"", slnFile);
			} else {
				slnFile = Path.Combine (project.ProjectDirectory, project.Title + ".Mac", project.Title + ".Mac.sln");
				args = String.Format ("build \"{0}\" -t:Build -c:\"Release|x86\"", slnFile);
			}
#elif WIN
			// Uncomment follow block if you would like to use mdtool instead of MSBuild
			/*
			app = @"C:\Program Files (x86)\MonoDevelop\bin\mdtool.exe";
			slnFile = Path.Combine (project.ProjectDirectory, project.Title + ".Win", project.Title + ".Win.sln");
			args = String.Format ("build \"{0}\" -t:Build -c:\"Release|x86\"", slnFile);
			*/

			app = Path.Combine (System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory (), "MSBuild.exe");
			slnFile = Path.Combine (project.ProjectDirectory, project.Title + ".Win", project.Title + ".Win.sln");
			args = String.Format ("\"{0}\" /verbosity:minimal /p:Configuration=Release", slnFile);
#endif
			if (StartProcess (app, args) != 0) {
				return false;
			}
#if MAC
			if (platform == TargetPlatform.Desktop) {
				string appName = Path.GetFileName (project.ProjectDirectory);
				string src = "/Applications/MonoDevelop.app/Contents/MacOS/lib/monodevelop/Addins";
				string dst = Path.Combine (project.ProjectDirectory, appName + ".Mac", "bin/Release", appName + ".app", "Contents/Resources");
				CopyFile (src, dst, "MonoMac.dll");
			}
#endif
			return true;
		}

		public bool Clean ()
		{
			Console.WriteLine ("------------- Cleanup Game Application -------------");
			string app, args, slnFile;
#if MAC
			app = "/Applications/MonoDevelop.app/Contents/MacOS/mdtool";
			if (platform == TargetPlatform.iOS) {
				slnFile = Path.Combine (project.ProjectDirectory, project.Title, project.Title + ".sln");
				args = String.Format ("build \"{0}\" -t:Clean -c:\"Release|iPhone\"", slnFile);
			} else {
				slnFile = Path.Combine (project.ProjectDirectory, project.Title, project.Title + ".sln");
				args = String.Format ("build \"{0}\" -t:Clean -c:\"Release|x86\"", slnFile);
			}
#elif WIN
			// Uncomment follow block if you would like to use mdtool instead of MSBuild
			/*
			app = @"C:\Program Files (x86)\MonoDevelop\bin\mdtool.exe";
			slnFile = Path.Combine (project.ProjectDirectory, project.Title + ".Win", project.Title + ".Win.sln");
			args = String.Format ("build \"{0}\" -t:Clean -c:\"Release|x86\"", slnFile);
			*/

			app = Path.Combine (System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory (), "MSBuild.exe");
			slnFile = Path.Combine (project.ProjectDirectory, project.Title + ".Win", project.Title + ".Win.sln");
			args = String.Format ("\"{0}\" /t:Clean /p:Configuration=Release", slnFile);
#endif
			if (StartProcess (app, args) != 0) {
				return false;
			}
			return true;
		}
		
		public void Run ()
		{
			Console.WriteLine ("------------- Starting Game -------------");
			string app, dir;
#if MAC
			if (platform == TargetPlatform.Desktop) {
				app = Path.Combine (project.ProjectDirectory, project.Title + ".Mac", "bin/Release", project.Title + ".app", "Contents/MacOS", project.Title);
				dir = Path.GetDirectoryName (app);
			} else {
				throw new NotImplementedException ();
			}
#elif WIN
			app = Path.Combine (project.ProjectDirectory, project.Title + ".Win", "bin/Release", project.Title + ".exe");
			dir = Path.GetDirectoryName (app);
#endif
			using (new DirectoryChanger (dir)) {
				StartProcess (app, "");
			}
		}
	}
}

