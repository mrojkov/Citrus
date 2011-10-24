using System;
using System.IO;

namespace Orange
{
	public static class ConfigConverter
	{
		public static void Convert (string srcPath, string dstPath, string gameProto)
		{
			// Get proto type using following rule:
			// xxx.txt -> xxx
			// xxx.yyy.txt -> yyy
			string type = Path.GetFileNameWithoutExtension (srcPath);
			if (Path.GetExtension (type) != "") {
				type = Path.GetExtension (type).Remove (0, 1);
			}
			string protoPath = Path.GetDirectoryName (gameProto);
			string args = String.Format ("--encode={0} --proto_path={1} {2}", type, protoPath, gameProto);
#if WIN
			string protoc = Path.Combine (Helpers.GetApplicationDirectory (), "ProtoCompiler.Win", "protoc.exe");
			var psi = new System.Diagnostics.ProcessStartInfo (protoc, args);
			psi.RedirectStandardInput = true;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardError = true;
			psi.UseShellExecute = false;
			psi.CreateNoWindow = true;
			var p = System.Diagnostics.Process.Start (psi);
#else
			string protoc = Path.Combine (Helpers.GetApplicationDirectory (), "ProtoCompiler.Mac", "protoc");
			var p = System.Diagnostics.Process.Start (protoc, args);
#endif
			// Send input data
			string buffer = File.ReadAllText (srcPath, System.Text.Encoding.Default);
			p.StandardInput.Write (buffer);
			p.StandardInput.Close ();

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
					String.Format ("Failed to compile config '{0}' (error code: {1})\nProtocol compile log:\n {2}", srcPath, p.ExitCode, errLog));
			}

			// Gathering output
			using (var output = new FileStream (dstPath, FileMode.Create)) {
				p.StandardOutput.BaseStream.CopyTo (output);
			}
		}
	}
}