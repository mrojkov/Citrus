using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Launcher
{
	internal abstract class CommonBuilder
	{
		private bool areFailedDetailsSet;

		public string SolutionPath;
		public string ExecutablePath;
		public Action<string> LoggingAction;
		
		public event Action<string> OnBuildStatusChange;
		public event Action OnBuildFail;
		public event Action OnBuildSuccess;
		
		private void RunExecutable()
		{
			var process = new Process {
				StartInfo = {
					FileName = ExecutablePath ?? DefaultExecutablePath
				}
			};
			process.Start();
		}

		public Task Start(bool runExecutable)
		{
			var task = new Task(() => BuildAndRun(runExecutable));
			task.Start();
			return task;
		}

		private void BuildAndRun(bool runExecutable)
		{
			var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
			while (currentDirectory.GetDirectories().All(d => d.Name != "Orange")) {
				if (currentDirectory.Parent == null) {
					SetFailedBuildStatus("Cannot find Orange directory");
				}
				currentDirectory = currentDirectory.Parent;
			}
			Environment.CurrentDirectory = Path.Combine(currentDirectory.FullName, "Orange");
			OnBuildStatusChange?.Invoke("Building");
			if (AreRequirementsMet() && Build(SolutionPath ?? DefaultSolutionPath)) {
				if (runExecutable) {
					RunExecutable();
				}
				OnBuildSuccess?.Invoke();
			}
			else {
				if (!areFailedDetailsSet)
					SetFailedBuildStatus("Send this text to our developers.");
				OnBuildFail?.Invoke();
			}
		}

		private bool Build(string solutionPath)
		{
			var process = new Process {
				StartInfo = {
					FileName = BuilderPath,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				}
			};
			process.OutputDataReceived += Builder_OnDataReceived;
			process.ErrorDataReceived += Builder_OnDataReceived;
			DecorateBuildProcess(process, solutionPath);
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			while (!process.HasExited) {
				process.WaitForExit(50);
			}
			return process.ExitCode == 0;
		}

		private void Builder_OnDataReceived(object sender, DataReceivedEventArgs args)
		{
			lock (this) {
				if (args.Data != null) {
					LoggingAction(args.Data);
				}
			}
		}

		protected virtual bool AreRequirementsMet() => true;

		protected virtual void DecorateBuildProcess(Process process, string solutionPath) { }

		protected void SetFailedBuildStatus(string details)
		{
			OnBuildStatusChange?.Invoke($"Build failed. {details}");
			areFailedDetailsSet = true;
		}
		
		protected abstract string DefaultSolutionPath { get; }
		protected abstract string DefaultExecutablePath { get; }
		protected abstract string BuilderPath { get; }
	}
}
