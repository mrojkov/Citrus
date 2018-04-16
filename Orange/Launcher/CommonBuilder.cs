using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Launcher
{
	internal abstract class CommonBuilder
	{
		private bool areFailedDetailsSet;

		public string SolutionPath;
		public string ExecutablePath;
		public string ExecutableArgs;

		public event Action<string> OnBuildStatusChange;
		public event Action OnBuildFail;
		public event Action OnBuildSuccess;

		private void RunExecutable()
		{
			var process = new Process {
				StartInfo = {
					FileName = ExecutablePath ?? DefaultExecutablePath,
					Arguments = ExecutableArgs
				}
			};
			process.Start();
		}

		private void RestoreNuget()
		{
#if WIN
			Orange.Nuget.Restore(Path.Combine(CalcCitrusDirectory(), "Orange/Orange.Win.sln"));
#elif MAC
			Orange.Nuget.Restore(Path.Combine(CalcCitrusDirectory(), "Orange/Orange.Mac.sln"));
#endif
		}

		private void SynchronizeAllProjects()
		{
			var citrus = CalcCitrusDirectory();
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Yuzu/Yuzu.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Yuzu/Yuzu.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Lime/Lime.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Lime/Lime.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Lime/Lime.MonoMac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Kumquat/Kumquat.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Kumquat/Kumquat.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Orange/Orange.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Orange/Orange.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Orange/Orange.CLI/Orange.Win.CLI.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Orange/Orange.CLI/Orange.Mac.CLI.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Orange/Orange.GUI/Orange.Win.GUI.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrus}/Orange/Orange.GUI/Orange.Mac.GUI.csproj");
		}

		public Task Start(bool runExecutable)
		{
			var task = new Task(() => {
				try {
					RestoreNuget();
					SynchronizeAllProjects();
					BuildAndRun(runExecutable);
				} catch (Exception e) {
					Console.WriteLine(e.Message);
					OnBuildFail?.Invoke();
				}
			});
			task.Start();
			return task;
		}

		private string CalcCitrusDirectory()
		{
			var path = Uri.UnescapeDataString((new Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);
			while (!string.Equals(Path.GetFileName(path), "Citrus", StringComparison.CurrentCultureIgnoreCase)) {
				path = Path.GetDirectoryName(path);
				if (string.IsNullOrEmpty(path)) {
					SetFailedBuildStatus("Cannot find Orange directory");
				}
			}
			return path;
		}

		private void BuildAndRun(bool runExecutable)
		{
			var citrusDirectory = CalcCitrusDirectory();
			Environment.CurrentDirectory = Path.Combine(citrusDirectory, "Orange");
			ClearObjFolder(citrusDirectory);
			OnBuildStatusChange?.Invoke("Building");
			if (AreRequirementsMet() && Build(SolutionPath ?? DefaultSolutionPath)) {
				ClearObjFolder(citrusDirectory);
				if (runExecutable) {
					RunExecutable();
				}
				OnBuildSuccess?.Invoke();
			} else {
				if (!areFailedDetailsSet) {
					SetFailedBuildStatus("Send this text to our developers.");
				}
				OnBuildFail?.Invoke();
			}
		}

		private static void ClearObjFolder(string citrusDirectory)
		{
			// Mac-specific bug: while building Lime.iOS mdtool reuses obj folder after Lime.MonoMac build,
			// which results in invalid Lime.iOS assembly (missing classes, etc.).
			// Solution: remove obj folder after Orange build (and before, just in case).
			var path = Path.Combine(citrusDirectory, "Lime", "obj");
			if (Directory.Exists(path)) {
				// https://stackoverflow.com/a/1703799
				foreach (var dir in Directory.EnumerateDirectories(path)) {
					ForceDeleteDirectory(dir);
				}
				ForceDeleteDirectory(path);
			}
		}

		private static void ForceDeleteDirectory(string path)
		{
			try {
				Directory.Delete(path, true);
			} catch (IOException) {
				Thread.Sleep(100);
				Directory.Delete(path, true);
			} catch (UnauthorizedAccessException) {
				Thread.Sleep(100);
				Directory.Delete(path, true);
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
					Console.WriteLine(args.Data);
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
