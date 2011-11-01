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
		
		public void Build ()
		{
			Console.WriteLine ("------------- Building Game Application -------------");
			string args = "", slnFile = "";
#if MAC
			string mdTool = "/Applications/MonoDevelop.app/Contents/MacOS/mdtool";
			if (platform == TargetPlatform.iOS) {
				string slnName = Path.GetFileName (projectFolder) + ".iOS";
				slnFile = Path.Combine (projectFolder, slnName, slnName + ".sln");
				args = String.Format ("build \"{0}\" -t:Build -c:\"Release|iPhone\"", slnFile);
			} else {
				string slnName = Path.GetFileName (projectFolder) + ".Mac";
				slnFile = Path.Combine (projectFolder, slnName, slnName + ".sln");
				args = String.Format ("build \"{0}\" -t:Build -c:\"Release|x86\"", slnFile);
			}
#endif
			var p = new System.Diagnostics.Process ();
			p.StartInfo.FileName = mdTool;
			p.StartInfo.Arguments = args;
			p.StartInfo.EnvironmentVariables.Clear ();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.StandardOutputEncoding = System.Text.Encoding.Default;
			p.StartInfo.StandardErrorEncoding = System.Text.Encoding.Default;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			p.Start ();
			while (!p.HasExited) {
				p.WaitForExit (100);
				while (p.StandardOutput.Peek () != -1) {
					Console.Write (p.StandardOutput.ReadLine ());
				}
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();
			}
			Console.Write (p.StandardOutput.ReadToEnd ());
			// var errors = p.StandardError.ReadToEnd ();
			if (p.ExitCode != 0) {
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
		
		public bool Run ()
		{
			Console.WriteLine ("------------- Starting Game -------------");
#if MAC
			string app = "", args = "", dir = Directory.GetCurrentDirectory ();
			if (platform == TargetPlatform.Desktop) {
				string appName = Path.GetFileName (projectFolder);
				app = Path.Combine (projectFolder, appName + ".Mac", "bin/Release", appName + ".app", "Contents/MacOS", appName);
				dir = Path.GetDirectoryName (app);
			} else {				
				throw new NotImplementedException ();
			}
#endif
			using (new DirectoryChanger (dir)) {
				var p = new System.Diagnostics.Process ();
				p.StartInfo.FileName = app;
				p.StartInfo.Arguments = args;
				p.StartInfo.EnvironmentVariables.Clear ();
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.StandardOutputEncoding = System.Text.Encoding.Default;
				p.StartInfo.StandardErrorEncoding = System.Text.Encoding.Default;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;
				p.Start ();
				while (!p.HasExited) {
					p.WaitForExit (100);
					while (p.StandardError.Peek () != -1) {
						Console.Write (p.StandardError.ReadLine ());
					}
					while (p.StandardOutput.Peek () != -1) {
						Console.Write (p.StandardOutput.ReadLine ());
					}
					while (Gtk.Application.EventsPending ())
						Gtk.Application.RunIteration ();
				}
				Console.Write (p.StandardError.ReadToEnd ());
				Console.Write (p.StandardOutput.ReadToEnd ());
				return p.ExitCode == 0;
			}
		}
	}
}

