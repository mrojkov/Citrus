using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using System.Text;
using System.Reflection;
using System.Threading;

namespace Launcher
{
	public class Builder
	{
		private string citrusDirectory;
		public bool NeedRunExecutable = true;
		public string SolutionPath;
		public string ExecutablePath;
		public string ExecutableArgs;

		public event Action<string> OnBuildStatusChange;
		public event Action OnBuildFail;
		public event Action OnBuildSuccess;

		public Builder(string citrusDirectory)
		{
			this.citrusDirectory = citrusDirectory;
		}

		private void RunExecutable()
		{
			var process = new Process {
				StartInfo = {
					FileName = ExecutablePath,
					Arguments = ExecutableArgs
				}
			};
			process.Start();
		}

		private void RestoreNuget()
		{
			Orange.Nuget.Restore(SolutionPath);
		}

		private void SynchronizeAllProjects()
		{
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Yuzu/Yuzu.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Yuzu/Yuzu.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Yuzu/Yuzu.iOS.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Yuzu/Yuzu.Android.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Lime/Lime.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Lime/Lime.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Lime/Lime.iOS.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Lime/Lime.Android.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Lime/Lime.MonoMac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Kumquat/Kumquat.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Kumquat/Kumquat.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Orange/Orange.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Orange/Orange.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Orange/Orange.CLI/Orange.Win.CLI.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Orange/Orange.CLI/Orange.Mac.CLI.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Orange/Orange.GUI/Orange.Win.GUI.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Orange/Orange.GUI/Orange.Mac.GUI.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.Core/Tangerine.Core.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.Core/Tangerine.Core.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.UI/Tangerine.UI.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.UI/Tangerine.UI.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.UI.FilesystemView/Tangerine.UI.FilesystemView.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.UI.FilesystemView/Tangerine.UI.FilesystemView.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.UI.Inspector/Tangerine.UI.Inspector.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.UI.Inspector/Tangerine.UI.Inspector.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.UI.SceneView/Tangerine.UI.SceneView.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.UI.SceneView/Tangerine.UI.SceneView.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.UI.Timeline/Tangerine.UI.Timeline.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{citrusDirectory}/Tangerine/Tangerine.UI.Timeline/Tangerine.UI.Timeline.Mac.csproj");
		}

		public Task Start()
		{
			var task = new Task(() => {
				try {
					RestoreNuget();
					SynchronizeAllProjects();
					BuildAndRun();
				} catch (Exception e) {
					Console.WriteLine(e.Message);
					SetFailedBuildStatus("");
				}
			});
			task.Start();
			return task;
		}

		private void BuildAndRun()
		{
			Environment.CurrentDirectory = Path.GetDirectoryName(SolutionPath);
			ClearObjFolder(citrusDirectory);
			OnBuildStatusChange?.Invoke("Building");
			if (AreRequirementsMet() && Build(SolutionPath)) {
				ClearObjFolder(citrusDirectory);
				if (NeedRunExecutable) {
					RunExecutable();
				}
				OnBuildSuccess?.Invoke();
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
					FileName = builderPath,
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
			bool result = process.ExitCode == 0;
			if (!result) {
				SetFailedBuildStatus($"Process exited with code {process.ExitCode}");
			}
			return result;
		}

		private void Builder_OnDataReceived(object sender, DataReceivedEventArgs args)
		{
			lock (this) {
				if (args.Data != null) {
					Console.WriteLine(args.Data);
				}
			}
		}

		private bool AreRequirementsMet()
		{
#if WIN
			if (builderPath != null) {
				return true;
			}
			Process.Start(@"https://visualstudio.microsoft.com/ru/thank-you-downloading-visual-studio/?sku=BuildTools&rel=15");
			SetFailedBuildStatus("Please install Microsoft Build Tools 2017");
			return false;
#else
			return true;
#endif // WIN
		}

		private void DecorateBuildProcess(Process process, string solutionPath)
		{
#if WIN
			process.StartInfo.Arguments =
				$"\"{solutionPath}\" /t:Build /p:Configuration=Release /p:Platform=x86 /verbosity:minimal";
			var cp = Encoding.Default.CodePage;
			if (cp == 1251)
				cp = 866;
			process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(cp);
			process.StartInfo.StandardErrorEncoding = Encoding.GetEncoding(cp);
#elif MAC
			process.StartInfo.Arguments = $"build \"{solutionPath}\" -t:Build -c:Release|x86";
#endif // WIN
		}

		private void SetFailedBuildStatus(string details = null)
		{
			if (string.IsNullOrEmpty(details)) {
				details = "Send this text to our developers.";
			}
			OnBuildStatusChange?.Invoke($"Build failed. {details}");
			OnBuildFail?.Invoke();
		}

#if WIN
		private string builderPath
		{
			get {
				var visualStudioRegistryPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7");
				if (visualStudioRegistryPath != null) {
					var vsPath = visualStudioRegistryPath.GetValue("15.0", string.Empty) as string;
					var vsBuild15Path = Path.Combine(vsPath, "MSBuild", "15.0", "Bin", "MSBuild.exe");
					if (File.Exists(vsBuild15Path)) {
						return vsBuild15Path;
					}
				}

				var msBuild15Path = Path.Combine(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\", "MSBuild.exe");
				if (File.Exists(msBuild15Path)) {
					return msBuild15Path;
				}

				var msBuild14Path = Path.Combine(@"C:\Program Files (x86)\MSBuild\14.0\Bin\", "MSBuild.exe");
				if (File.Exists(msBuild14Path)) {
					return msBuild14Path;
				}

				return null;
			}
		}
#elif MAC
		private string builderPath
		{
				get {
				var mdtool = "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool";
				var vstool = "/Applications/Visual Studio.app/Contents/MacOS/vstool";

				if (File.Exists(vstool)) {
					return vstool;
				} else {
					return mdtool;
				}
			}
		}
#endif // WIN
}
}
