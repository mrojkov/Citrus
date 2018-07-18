using System;
using System.IO;
using System.Text;
using Orange.Source;
using System.Linq;
using Exception = Lime.Exception;

namespace Orange
{
	public class SolutionBuilder
	{
		private readonly BuildSystem buildSystem;

		private readonly string projectDirectory;
		private readonly string projectName;


		public SolutionBuilder(
			TargetPlatform platform, string solutionPath, string configuration = BuildConfiguration.Release)
		{
#if WIN
			buildSystem = new MSBuild(platform, solutionPath, configuration);
#elif MAC
			buildSystem = new MDTool(platform, solutionPath, configuration);
#else
			throw new NotSupportedException();
#endif
			projectDirectory = Path.GetDirectoryName(buildSystem.SolutionPath);
			projectName = Path.GetFileNameWithoutExtension(projectDirectory);
		}

		public void SvnUpdate()
		{
			Subversion.Update(Path.GetDirectoryName(The.Workspace.GetLimeCsprojFilePath()));
			Subversion.Update(Path.GetDirectoryName(projectDirectory));
		}

		public static void CopyFile(string srcDir, string dstDir, string fileName)
		{
			var srcFile = Path.Combine(srcDir, fileName);
			var dstFile = Path.Combine(dstDir, fileName);
			Console.WriteLine("Copying: {0}", dstFile);
			File.Copy(srcFile, dstFile, true);
		}

		private static void SynchronizeAll()
		{
			var dontSynchronizeProject = The.Workspace.ProjectJson["DontSynchronizeProject"] as bool?;
			if (dontSynchronizeProject != null && dontSynchronizeProject.Value) {
				return;
			}
			var fileEnumerator = new ScanOptimizedFileEnumerator(
				The.Workspace.ProjectDirectory,
				CsprojSynchronization.SkipUnwantedDirectoriesPredicate,
				cutDirectoryPrefix: false
			);
			foreach (var target in The.Workspace.Targets) {
				var limeProj = CsprojSynchronization.ToUnixSlashes(The.Workspace.GetLimeCsprojFilePath(target.Platform));
				CsprojSynchronization.SynchronizeProject(limeProj);
				using (new DirectoryChanger(The.Workspace.ProjectDirectory)) {
					var dirInfo = new System.IO.DirectoryInfo(The.Workspace.ProjectDirectory);
					foreach (var fileInfo in fileEnumerator.Enumerate(The.Workspace.GetPlatformSuffix(target.Platform) + ".csproj")) {
						CsprojSynchronization.SynchronizeProject(fileInfo.Path);
					};
					if (target.ProjectPath != null) {
						foreach (var targetCsprojFile in fileEnumerator.Enumerate(Path.GetFileName(target.ProjectPath))) {
							CsprojSynchronization.SynchronizeProject(targetCsprojFile.Path);
						}
					}
				}
			}
		}

		public bool Build(StringBuilder output = null)
		{
			Console.WriteLine("------------- Building Application -------------");
			SynchronizeAll();
			var nugetResult = Nuget.Restore(projectDirectory);
			if (nugetResult != 0) {
				Console.WriteLine("NuGet exited with code: {0}", nugetResult);
			}
			return (buildSystem.Execute(BuildAction.Build, output) == 0);
		}

		public bool Restore()
		{
			Console.WriteLine("------------- Restore Application -------------");
			return (buildSystem.Execute(BuildAction.Restore) == 0);
		}

		public bool Clean()
		{
			Console.WriteLine("------------- Cleanup Game Application -------------");
			return (buildSystem.Execute(BuildAction.Clean) == 0);
		}

		public string GetApplicationPath()
		{
#if MAC
			if (buildSystem.Platform == TargetPlatform.Mac) {
				return Path.Combine(
					projectDirectory,
					"bin",
					buildSystem.Configuration,
					projectName + ".app",
					"Contents/MacOS",
					projectName);
			} else {
				throw new NotImplementedException();
			}
#elif WIN
			return Path.Combine(
				projectDirectory,
				"bin",
				buildSystem.Configuration,
				projectName + ".exe");
#endif
		}

		public int Run(string arguments)
		{
			Console.WriteLine("------------- Starting Application -------------");

			if (buildSystem.Platform == TargetPlatform.Android) {
				var signedApks = Directory.GetFiles(buildSystem.BinariesDirectory)
					.Where(file => file.EndsWith("-Signed.apk"));

				if (signedApks.Count() != 1) {
					Console.WriteLine("There must be single signed apk file in binary's folder");
					return 1;
				}
				AdbDeploy(signedApks.First());
				return 0;
			}
#if WIN
			var applicationPath = GetApplicationPath();
			if (File.Exists(applicationPath)) {
				using (new DirectoryChanger(Path.GetDirectoryName(applicationPath))) {
					return Process.Start(applicationPath, arguments);
				}
			} else {
				Console.WriteLine("Error: File not found: " + applicationPath);
				return 1;
			}
#else
			if (buildSystem.Platform == TargetPlatform.Mac) {
				return Process.Start(GetMacAppName(), string.Empty);
			} else {
				var args = "--installdev=" + GetIOSAppName();
				int exitCode = Process.Start("/Developer/MonoTouch/usr/bin/mtouch", args);
				if (exitCode != 0) {
					return exitCode;
				}
				Console.WriteLine("Please start app manually :)");
				return exitCode;
			}
#endif
		}

		private string GetIOSAppName()
		{
			var directory = Path.Combine(
				projectDirectory, "bin", "iPhone", buildSystem.Configuration);
			var all = new DirectoryInfo(directory).EnumerateDirectories("*.app");
			if (all.Any()) {
				var allSorted = all.ToList();
				allSorted.Sort((a, b) => b.CreationTime.CompareTo(a.CreationTime));
				return Path.Combine(directory, allSorted[0].FullName);
			}
			return null;
		}

		private string GetMacAppName()
		{
			var directory = Path.Combine(projectDirectory, "bin", buildSystem.Configuration);
			var all = new DirectoryInfo(directory).EnumerateDirectories("*.app");
			if (all.Any()) {
				var allSorted = all.ToList();
				allSorted.Sort((a, b) => b.CreationTime.CompareTo(a.CreationTime));
				var firstFileName = allSorted[0].FullName;
				return Path.Combine(
					directory,
					firstFileName,
					"Contents",
					"MacOS",
					Path.GetFileNameWithoutExtension(firstFileName));
			}
			return null;
		}

		private static void AdbDeploy(string apkPath)
		{
			var adb = GetAdbPath();
			var packageName = Path.GetFileNameWithoutExtension(apkPath);

			var signedIndex = packageName.IndexOf("-Signed");
			if (signedIndex != -1)
				packageName = packageName.Substring(0, signedIndex);

			Console.WriteLine("------------------ Deploying ------------------");
			Console.WriteLine($"Uninstalling previous apk ({packageName})");

			if (Process.Start(adb, $"shell pm uninstall {packageName}") == 0) {
				Console.WriteLine("Uninstalled!");
			} else {
				Console.WriteLine("Error during uninstalling. Probably application wasn't installed.");
			}

			Console.WriteLine($"Installing apk {apkPath}");
			if (Process.Start(adb, $"install {apkPath}") == 0) {
				Console.WriteLine("App installed.");
				Console.WriteLine("Starting application.");
				Process.Start(adb, $"shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1");
			} else {
				Console.WriteLine("Error during installing.");
			}
		}

		private static string GetAdbPath()
		{
			string androidSdk = Toolbox.GetCommandLineArg("--android-sdk");
			string executable = "adb";

			if (androidSdk == null) {
#if WIN
				var appData = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%");
				androidSdk = Path.Combine(appData, "Android", "android-sdk");
				executable = Path.Combine(androidSdk, "platform-tools", "adb.exe");
#elif MAC
				androidSdk = ""; // TODO: Find defualt sdk path on OSX and assign executable
#endif
			}

			if (File.Exists(executable)) {
				return executable;
			}
			throw new Exception(
				"ADB not found. You can specify sdk location with" +
				$"--android-sdk argument. Used sdk path: {androidSdk}. ");
		}
	}
}
