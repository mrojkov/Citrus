using System;
using System.IO;

namespace Orange
{
	public class SolutionBuilder
	{
		private string projectFolder;
		private TargetPlatform platform;
		
		public SolutionBuilder (string projectFolder, TargetPlatform platform)
		{
			this.projectFolder = projectFolder;
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
#else
			p.StartInfo.StandardOutputEncoding = System.Text.Encoding.Default;
			p.StartInfo.StandardErrorEncoding = System.Text.Encoding.Default;
			p.StartInfo.EnvironmentVariables.Clear ();
#endif
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			var logger = new System.Text.StringBuilder ();
			p.OutputDataReceived += (sender, e) => { lock (logger) { logger.AppendLine (e.Data); } };
			p.ErrorDataReceived += (sender, e) => { lock (logger) { logger.AppendLine (e.Data); } };
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
		
		public void Build ()
		{
			Console.WriteLine ("------------- Building Game Application -------------");
			string app, args, slnFile;
#if MAC
			app = "/Applications/MonoDevelop.app/Contents/MacOS/mdtool";
			if (platform == TargetPlatform.iOS) {
				string slnName = Path.GetFileName (projectFolder) + ".iOS";
				slnFile = Path.Combine (projectFolder, slnName, slnName + ".sln");
				args = String.Format ("build \"{0}\" -t:Build -c:\"Release|iPhone\"", slnFile);
			} else {
				string slnName = Path.GetFileName (projectFolder) + ".Mac";
				slnFile = Path.Combine (projectFolder, slnName, slnName + ".sln");
				args = String.Format ("build \"{0}\" -t:Build -c:\"Release|x86\"", slnFile);
			}
#elif WIN
			// Uncomment follow block if you would like to use mdtool instead of MSBuild
			/*
			app = @"C:\Program Files (x86)\MonoDevelop\bin\mdtool.exe";
			string slnName = Path.GetFileName (projectFolder) + ".Win";
			slnFile = Path.Combine (projectFolder, slnName, slnName + ".sln");
			args = String.Format ("build \"{0}\" -t:Build -c:\"Release|x86\"", slnFile);
			*/

			app = Path.Combine (System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory (), "MSBuild.exe");
			string slnName = Path.GetFileName (projectFolder) + ".Win";
			slnFile = Path.Combine (projectFolder, slnName, slnName + ".sln");
			args = String.Format ("\"{0}\" /verbosity:minimal /p:Configuration=Release", slnFile);
#endif
			if (StartProcess (app, args) != 0) {
				throw new Lime.Exception ("Build failed");
			}
#if MAC
			if (platform == TargetPlatform.Desktop) {
				string appName = Path.GetFileName (projectFolder);
				string src = "/Applications/MonoDevelop.app/Contents/MacOS/lib/monodevelop/Addins";
				string dst = Path.Combine (projectFolder, appName + ".Mac", "bin/Release", appName + ".app", "Contents/Resources");
				CopyFile (src, dst, "MonoMac.dll");
			}
#endif
		}
		
		public void Run ()
		{
			Console.WriteLine ("------------- Starting Game -------------");
			string app, dir;
			string appName = Path.GetFileName (projectFolder);
#if MAC
			if (platform == TargetPlatform.Desktop) {
				app = Path.Combine (projectFolder, appName + ".Mac", "bin/Release", appName + ".app", "Contents/MacOS", appName);
				dir = Path.GetDirectoryName (app);
			} else {
				throw new NotImplementedException ();
			}
#elif WIN
			app = Path.Combine (projectFolder, appName + ".Win", "bin/Release", appName + ".exe");
			dir = Path.GetDirectoryName (app);
#endif
			using (new DirectoryChanger (dir)) {
				StartProcess (app, "");
			}
		}
	}
}

