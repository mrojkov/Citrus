using System;
using System.IO;

namespace Orange
{
	class ProtoGen
	{
		public static void Execute (string protoPath)
		{
			using (new DirectoryChanger (Path.GetDirectoryName (protoPath))) {
				protoPath = Path.GetFileName (protoPath);
				string csPath = Path.ChangeExtension (protoPath, ".cs");
				if (File.Exists (csPath) && File.GetLastWriteTime (csPath) > File.GetLastWriteTime (protoPath))
					return;
				Console.WriteLine ("Updating {0}", Path.Combine (Directory.GetCurrentDirectory (), csPath));
				string args = String.Format ("-i:\"{0}\" -o:\"{1}\"", protoPath, csPath);
#if WIN
				string protogen = Path.Combine (Helpers.GetApplicationDirectory (), "ToolChain.Win", "protogen.exe");
#else
				string protogen = Path.Combine (Helpers.GetApplicationDirectory (), "ToolChain.Mac", "protogen");
#endif
				var psi = new System.Diagnostics.ProcessStartInfo (protogen, args);
				psi.RedirectStandardError = true;
				psi.UseShellExecute = false;
				psi.CreateNoWindow = true;
				var p = System.Diagnostics.Process.Start (psi);

				// Wait for complier 
				while (!p.HasExited) {
					p.WaitForExit ();
				}
				// Check for errors
				if (p.ExitCode != 0) {
					string errLog = "";
					string msg;
					while ((msg = p.StandardError.ReadLine ()) != null) {
						errLog += "\t" + msg + "\n";
					}
					throw new Lime.Exception (
						String.Format ("Failed to generate C# proto binding '{0}' (error code: {1})\nCompile log:\n {2}", protoPath, p.ExitCode, errLog));
				}
			}
		}
	}
}
