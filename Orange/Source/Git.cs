using System;
using System.Diagnostics;
using System.Threading;
using Lime;

namespace Orange
{
	public static class Git
	{
		public static bool Exec(string gitDir, string gitArgs)
		{
			return Exec(gitDir, gitArgs, out var stdout, out var stderr);
		}

		public static bool Exec(string gitDir, string gitArgs, out string stdout, out string stderr)
		{
			var process = new System.Diagnostics.Process {
				StartInfo = {
					FileName = "git",
					Arguments = gitArgs,
					UseShellExecute = false,
					WorkingDirectory = gitDir,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
		}
			};
			// Terekhov Dmitry: cause out parameters can't be used in closure
			string stdoutAccumulator = "", stderrAccumulator = "";
			Console.WriteLine($"git {gitArgs}");
			process.OutputDataReceived += (sender, args) => {
				if (args.Data != null) {
					stdoutAccumulator += args.Data;
					Console.WriteLine(args.Data);
				}
			};
			process.ErrorDataReceived += (sender, args) => {
				if (args.Data != null) {
					stderrAccumulator += args.Data;
					Console.WriteLine(args.Data);
				}
			};
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit();
			stdout = stdoutAccumulator;
			stderr = stderrAccumulator;
			return process.ExitCode != 0;
		}

		public static string GetCurrentBranch(string gitDir)
		{
			Exec(gitDir, "rev-parse --abbrev-ref HEAD", out var stdout, out var stderr);
			return stdout.Trim();
		}

		public static void ForceUpdate(string gitDir)
		{
			new Thread(() => {
				var branch = GetCurrentBranch(gitDir);
				Exec(gitDir, $"fetch origin");
				Exec(gitDir, $"reset --hard origin/{branch}");
			}).Start();
		}

		public static void Update(string gitDir)
		{
			new Thread(() => {
				var branch = GetCurrentBranch(gitDir);
				Exec(gitDir, $"pull --ff-only origin {branch}");
			}).Start();
		}
	}
}
