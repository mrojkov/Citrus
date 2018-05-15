using System;
using System.IO;
using System.IO.Compression;
using McMaster.Extensions.CommandLineUtils;
using System.Linq;
using Octokit;
using Orange;
using Application = System.Windows.Forms.Application;
#if WIN
using System.Runtime.InteropServices;
#elif MAC
using AppKit;
#endif // WIN

namespace Orange
{
	public class UserInterface
	{
		public void ProcessPendingEvents() { }
		public static UserInterface Instance = new UserInterface();
	}
	public static class The
	{
		public static UserInterface UI { get { return UserInterface.Instance; } }
	}
}

namespace Launcher
{
	internal static class MainClass
	{
#if WIN
		[StructLayout(LayoutKind.Sequential)]
		internal struct PROCESS_INFORMATION
		{
			public IntPtr hProcess;
			public IntPtr hThread;
			public int dwProcessId;
			public int dwThreadId;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct STARTUPINFO
		{
			public Int32 cb;
			public string lpReserved;
			public string lpDesktop;
			public string lpTitle;
			public Int32 dwX;
			public Int32 dwY;
			public Int32 dwXSize;
			public Int32 dwYSize;
			public Int32 dwXCountChars;
			public Int32 dwYCountChars;
			public Int32 dwFillAttribute;
			public Int32 dwFlags;
			public Int16 wShowWindow;
			public Int16 cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(StandardHandle nStdHandle);
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		static extern IntPtr GetCommandLineW();
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		static extern bool CreateProcessW(
			IntPtr lpApplicationName,
			IntPtr lpCommandLine,
			IntPtr lpProcessAttributes,
			IntPtr lpThreadAttributes,
			bool bInheritHandles,
			uint dwCreationFlags,
			IntPtr lpEnvironment,
			IntPtr lpCurrentDirectory,
			[In] ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation);


		const int SW_HIDE = 0;
		const int SW_SHOW = 5;

		private enum StandardHandle : uint
		{
			Input = unchecked((uint)-10),
			Output = unchecked((uint)-11),
			Error = unchecked((uint)-12)
		}

		const int STARTF_USESHOWWINDOW = 0x00000001;
		const int STARTF_USESIZE = 0x00000002;
		const int STARTF_USEPOSITION = 0x00000004;
		const int STARTF_USECOUNTCHARS = 0x00000008;
		const int STARTF_USEFILLATTRIBUTE = 0x00000010;
		const int STARTF_RUNFULLSCREEN = 0x00000020;  // ignored for non-x86 platforms
		const int STARTF_FORCEONFEEDBACK = 0x00000040;
		const int STARTF_FORCEOFFFEEDBACK = 0x00000080;
		const int STARTF_USESTDHANDLES = 0x00000100;

		private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
#endif // WIN

		private static Builder builder;

		[STAThread]
		public static int Main(string[] args)
		{
			var originalArgs = args;
#if MAC
			args = args.Where(s => !s.StartsWith("-psn")).ToArray();
#endif // MAC
			var cli = new CommandLineApplication();
			cli.Name = "Orange";
			cli.Description = "Orange Launcher";
			cli.HelpOption("-h --help");
			var optionConsole = cli.Option<bool>("-c --console", "Console mode.", CommandOptionType.NoValue);
			var optionJustBuild = cli.Option<bool>("-j --justbuild", "Build project without running executable.", CommandOptionType.NoValue);
			var optionBuildProjectPath = cli.Option<string>("-b --build <PROJECT_PATH>", "Project path, default: \"Orange/Orange.%Platform%.sln\".", CommandOptionType.SingleValue);
			var optionExecutablePath = cli.Option<string>("-r --run <EXECUTABLE_PATH>", "Executable path, default: \"Orange/bin/%Platform%/Release/%PlatformExecutable%\".", CommandOptionType.SingleValue);
			var optionRunArgs = cli.Option<string>("-a --runargs <ARGUMENTS>", "Args to pass to executable.", CommandOptionType.SingleValue);

			cli.Command("release", (releaseCommand) => {
				releaseCommand.HelpOption("-h --help");
				releaseCommand.Description = "Release provided Citrus bundle on github.";
				var githubUserOption = releaseCommand.Option<string>("-u --user <GITHUB_USER_NAME>", "github user name", CommandOptionType.SingleValue);
				githubUserOption.IsRequired();
				var githubPasswordOption = releaseCommand.Option<string>("-p --password <GITHUB_PASSWORD>", "github password", CommandOptionType.SingleValue);
				githubPasswordOption.IsRequired();
				var winBundlePath = releaseCommand.Option<string>("-w --win-bundle <WINDOWS_BUNDLE_PATH>", "Path to windows bundle of Citrus.", CommandOptionType.SingleValue);
				winBundlePath.IsRequired();
				var macBundlePath = releaseCommand.Option<string>("-m --mac-bundle <MAC_BUNDLE_PATH>", "Path to MAC OS bundle of Citrus.", CommandOptionType.SingleValue);
				macBundlePath.IsRequired();
				releaseCommand.OnExecute(async () => {
#if MAC
					NSApplication.Init();
#endif // MAC
					var client = new GitHubClient(new ProductHeaderValue(githubUserOption.ParsedValue));
					var basicAuth = new Credentials(githubUserOption.ParsedValue, githubPasswordOption.ParsedValue);
					client.Credentials = basicAuth;
					var release = new NewRelease("v1.0.0") {
						Name = "Test Release Name",
						Body = "#Header \n\n body text",
						Draft = false,
						Prerelease = false
					};
					var result = await client.Repository.Release.Create("mrojkov", "Citrus", release);
					Console.WriteLine("Created release id {0}", result.Id);
					var releases = await client.Repository.Release.GetAll("mrojkov", "Citrus");
					var latest = releases[0];
					Console.WriteLine(
						"The latest release is tagged at {0} and is named {1}",
						latest.TagName,
						latest.Name);

					var archiveContents = File.OpenRead(Path.Combine("temp_apth_here", "release.zip"));
					var assetUpload = new ReleaseAssetUpload() {
						FileName = "mrelease.zip",
						ContentType = "application/zip",
						RawData = archiveContents
					};
					var asset = await client.Repository.Release.UploadAsset(latest, assetUpload);
				});
			});

			cli.Command("bundle", (bundleCommand) => {
				bundleCommand.HelpOption("-h --help");
				bundleCommand.Description = "Build Tangerine, Orange and bundle them together into zip.";
				var tempOption = bundleCommand.Option<string>("-t --temp-directory <DIRECTORY_PATH>", "Temporary directory.", CommandOptionType.SingleValue);
				var outputOption = bundleCommand.Option<string>("-o --output <OUTPUT_DIRECTORY>", "Output directory.", CommandOptionType.SingleValue);
				bundleCommand.OnExecute(() => {
#if MAC
					NSApplication.Init();
#endif // MAC
					builder = new Builder { NeedRunExecutable = false };
					builder.OnBuildStatusChange += Console.WriteLine;
					builder.OnBuildFail += () => Environment.Exit(1);
					builder.Start().Wait();
					var tangerineBuilder = new Builder {
						SolutionPath = Path.Combine(builder.CitrusDirectory, "Tangerine", "Tangerine.Win.sln"),
						NeedRunExecutable = false
					};
					tangerineBuilder.OnBuildStatusChange += Console.WriteLine;
					tangerineBuilder.OnBuildFail += () => Environment.Exit(1);
					tangerineBuilder.Start().Wait();
					var orangeBinDir = Path.Combine(builder.CitrusDirectory, "Orange", "bin", "win", "Release");
					var tangerineBinDir = Path.Combine(builder.CitrusDirectory, "Tangerine", "bin", "Release");
					var orangeFiles = new FileEnumerator(orangeBinDir);
					var tangerineFiles = new FileEnumerator(tangerineBinDir);

					var tempPath = tempOption.HasValue() ? tempOption.ParsedValue : Path.Combine(builder.CitrusDirectory, "launcher_temp");
					var outputPath = outputOption.HasValue() ? outputOption.ParsedValue : Path.Combine(builder.CitrusDirectory, "launcher_output");
					if (Directory.Exists(tempPath)) {
						Directory.Delete(tempPath, true);
					}
					if (Directory.Exists(outputPath)) {
						Directory.Delete(outputPath, true);
					}
					Directory.CreateDirectory(tempPath);
					Directory.CreateDirectory(outputPath);
					foreach (var fi in orangeFiles.Enumerate()) {
						Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(tempPath, fi.Path)));
						File.Copy(Path.Combine(orangeBinDir, fi.Path), Path.Combine(tempPath, fi.Path));
					}
					foreach (var fi in tangerineFiles.Enumerate()) {
						Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(tempPath, fi.Path)));
						File.Copy(Path.Combine(tangerineBinDir, fi.Path), Path.Combine(tempPath, fi.Path), true);
					}
					File.Copy(Path.Combine(builder.CitrusDirectory, Orange.CitrusVersion.Filename), Path.Combine(tempPath, Orange.CitrusVersion.Filename));
					ZipFile.CreateFromDirectory(tempPath, Path.Combine(outputPath, "bundle.zip"), CompressionLevel.Optimal, false);
				});
			});

			cli.OnExecute(() => {
#if WIN
				var stdoutHandle = GetStdHandle(StandardHandle.Output);
				if (args.Length == 0 && stdoutHandle != INVALID_HANDLE_VALUE) {
					PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
					STARTUPINFO si = new STARTUPINFO();
					si.cb = Marshal.SizeOf(si);
					si.dwFlags = STARTF_USESHOWWINDOW | STARTF_USESTDHANDLES;
					si.wShowWindow = SW_SHOW;
					si.hStdOutput = INVALID_HANDLE_VALUE;
					si.hStdInput = INVALID_HANDLE_VALUE;
					si.hStdError = INVALID_HANDLE_VALUE;
					CreateProcessW(IntPtr.Zero,
						GetCommandLineW(),
						IntPtr.Zero,
						IntPtr.Zero,
						true,
						0x00000008, // DETACHED_PROCESS
						IntPtr.Zero,
						IntPtr.Zero,
						ref si,
						out pi
					);
					return 0;
				}
#endif // WIN
				string solutionPath = null;
				string executablePath = null;
				if (optionBuildProjectPath.HasValue()) {
					solutionPath = Path.Combine(Environment.CurrentDirectory, optionBuildProjectPath.ParsedValue);
				}
				if (optionExecutablePath.HasValue()) {
					executablePath = Path.Combine(Environment.CurrentDirectory, optionExecutablePath.ParsedValue);
				}
				builder = new Builder {
					NeedRunExecutable = !optionJustBuild.HasValue(),
					SolutionPath = solutionPath,
					ExecutablePath = executablePath,
					ExecutableArgs = optionRunArgs.ParsedValue
				};

				if (optionConsole.HasValue()) {
					StartConsoleMode();
				} else {
					// OS X passes `-psn_<number>` to process when start from Finder, so we cut it for
					// cli parser, but pass original args to OS X's NSApplication.Main
					StartUIMode(originalArgs);
				}
				return 0;
			});

			try {
				cli.Execute(args);
			} catch (CommandParsingException e) {
				Console.WriteLine(e.Message);
				return 1;
			}
			return 0;
		}

#if WIN
		private static void StartUIMode(string[] args)
		{
			var mainForm = new MainForm();
			builder.OnBuildStatusChange += mainForm.SetBuildStatus;
			builder.OnBuildFail += mainForm.ShowLog;
			builder.OnBuildSuccess += Application.Exit;
			Console.SetOut(mainForm.LogWriter);
			Console.SetError(mainForm.LogWriter);
			builder.Start();
			mainForm.Show();
			Application.Run();
		}
#elif MAC
		private static void StartUIMode(string[] args)
		{
			AppDelegate.Builder = builder;
			NSApplication.Init();
			NSApplication.Main(args);
		}
#endif // WIN

		private static void StartConsoleMode()
		{
#if MAC
			NSApplication.Init();
#endif // MAC
			builder.OnBuildStatusChange += Console.WriteLine;
			builder.OnBuildFail += () => Environment.Exit(1);
			builder.Start().Wait();
		}
	}
}
