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
		public string CitrusDirectory
		{
			get {
				if (string.IsNullOrEmpty(citrusDirectory)) {
					var path = Uri.UnescapeDataString((new Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);
					while (!string.Equals(Path.GetFileName(path), "Citrus", StringComparison.CurrentCultureIgnoreCase)) {
						path = Path.GetDirectoryName(path);
						if (string.IsNullOrEmpty(path)) {
							throw new InvalidOperationException("Cannot find Orange directory");
						}
					}
					citrusDirectory = path;
				}
				return citrusDirectory;
			}
		}
		private string OrangeDirectory { get { return Path.Combine(CitrusDirectory, "Orange"); } }
		public bool NeedRunExecutable = true;
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
			Orange.Nuget.Restore(Path.Combine(OrangeDirectory, "Orange.Win.sln"));
#elif MAC
			Orange.Nuget.Restore(Path.Combine(OrangeDirectory, "Orange.Mac.sln"));
#endif
		}

		private void SynchronizeAllProjects()
		{
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Yuzu/Yuzu.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Yuzu/Yuzu.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Lime/Lime.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Lime/Lime.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Lime/Lime.MonoMac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Kumquat/Kumquat.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Kumquat/Kumquat.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Orange/Orange.Win.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Orange/Orange.Mac.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Orange/Orange.CLI/Orange.Win.CLI.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Orange/Orange.CLI/Orange.Mac.CLI.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Orange/Orange.GUI/Orange.Win.GUI.csproj");
			Orange.CsprojSynchronization.SynchronizeProject($"{CitrusDirectory}/Orange/Orange.GUI/Orange.Mac.GUI.csproj");
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
			Environment.CurrentDirectory = OrangeDirectory;
			ClearObjFolder(CitrusDirectory);
			OnBuildStatusChange?.Invoke("Building");
			if (AreRequirementsMet() && Build(SolutionPath ?? DefaultSolutionPath)) {
				ClearObjFolder(CitrusDirectory);
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
			Process.Start(@"https://www.microsoft.com/en-us/download/details.aspx?id=48159");
			SetFailedBuildStatus("Please install Microsoft Build Tools 2015");
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

		private string DefaultSolutionPath =>
#if WIN
			Path.Combine(OrangeDirectory, "Orange.Win.sln");
#elif MAC
			Path.Combine(OrangeDirectory, "Orange.Mac.sln");
#endif // WIN

		private string DefaultExecutablePath =>
#if WIN
			Path.Combine(OrangeDirectory, @"bin\Win\Release\Orange.GUI.exe");
#elif MAC
			Path.Combine(OrangeDirectory, @"bin/Mac/Release/Orange.GUI.app/Contents/MacOS/Orange.GUI");
#endif // WIN

#if WIN
		private string builderPath
		{
			get {
				var msBuild14Path = Path.Combine(@"C:\Program Files (x86)\MSBuild\14.0\Bin\", "MSBuild.exe");
				if (File.Exists(msBuild14Path)) {
					return msBuild14Path;
				}

				var visualStudioRegistryPath =
					Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7");
				if (visualStudioRegistryPath != null) {
					var vsPath = visualStudioRegistryPath.GetValue("15.0", string.Empty) as string;
					var msBuild15Path = Path.Combine(vsPath, "MSBuild", "15.0", "Bin", "MSBuild.exe");
					if (File.Exists(msBuild15Path)) {
						return msBuild15Path;
					}
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

				if (File.Exists(mdtool)) {
					return mdtool;
				} else {
					return vstool;
				}
			}
		}
#endif // WIN
}
}
