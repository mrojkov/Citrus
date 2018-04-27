using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
#if WIN
using System.Windows.Forms;
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
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AttachConsole(int dwProcessId);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(StandardHandle nStdHandle);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetStdHandle(StandardHandle nStdHandle, IntPtr handle);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern FileType GetFileType(IntPtr handle);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool FreeConsole();

		private enum StandardHandle : uint
		{
			Input = unchecked((uint)-10),
			Output = unchecked((uint)-11),
			Error = unchecked((uint)-12)
		}

		private enum FileType : uint
		{
			Unknown = 0x0000,
			Disk = 0x0001,
			Char = 0x0002,
			Pipe = 0x0003
		}

		private static bool IsRedirected(IntPtr handle)
		{
			FileType fileType = GetFileType(handle);
			return (fileType == FileType.Disk) || (fileType == FileType.Pipe);
		}

		public static void Redirect()
		{
			bool errorRedirected = IsRedirected(GetStdHandle(StandardHandle.Error));
			AttachConsole(-1);
			if (!errorRedirected) {
				SetStdHandle(StandardHandle.Error, GetStdHandle(StandardHandle.Output));
			}
		}
#endif // WIN

		private static CommonBuilder builder;

		[STAThread]
		public static int Main(string[] args)
		{
#if WIN
			Redirect();
#endif // WIN
#if MAC
			args = args.Where(s => !s.StartsWith("-psn")).ToArray();
#endif // MAC
			var cli = new CommandLineApplication();
			cli.Name = "Orange";
			cli.HelpOption("-h --help");
			var optionConsole = cli.Option<bool>("-c --console", "Console mode.", CommandOptionType.NoValue);
			var optionJustBuild = cli.Option<bool>("-j --justbuild", "Build project without running executable.", CommandOptionType.NoValue);
			var optionBuildProjectPath = cli.Option<string>("-b --build <PROJECT_PATH>", "Project path, default: \"Orange/Orange.%Platform%.sln\".", CommandOptionType.SingleValue);
			var optionRunProjectPath = cli.Option<string>("-r --run <EXECUTABLE_PATH>", "Executable path, default: \"Orange/bin/%Platform%/Release/%PlatformExecutable%\".", CommandOptionType.SingleValue);
			var optionRunArgs = cli.Option<string>("-a --runargs <ARGUMENTS>", "Args to pass to executable.", CommandOptionType.SingleValue);

			cli.OnExecute(() =>
			{
				RunExecutable = !optionJustBuild.HasValue();
				builder = new Builder {
					SolutionPath = optionBuildProjectPath.ParsedValue,
					ExecutablePath = optionRunProjectPath.ParsedValue,
					ExecutableArgs = optionRunArgs.ParsedValue
				};

				if (optionConsole.HasValue()) {
					StartConsoleMode();
				} else {
					StartUIMode(args);
				}
				return 0;
			});

			try {
				cli.Execute(args);
			} catch (CommandParsingException e) {
				Console.WriteLine(e.Message);
				return 1;
			}
			FreeConsole();
			return 0;
		}

		private static bool RunExecutable;

#if WIN
		private static void StartUIMode(string[] args)
		{
			var mainForm = new MainForm();
			builder.OnBuildStatusChange += mainForm.SetBuildStatus;
			builder.OnBuildFail += mainForm.ShowLog;
			builder.OnBuildSuccess += Application.Exit;
			Console.SetOut(mainForm.LogWriter);
			Console.SetError(mainForm.LogWriter);
			builder.Start(RunExecutable);
			mainForm.Show();
			Application.Run();
		}
#elif MAC
		private static void StartUIMode(string[] args)
		{
			AppDelegate.Builder = builder;
			AppDelegate.Args = Args;
			NSApplication.Init();
			NSApplication.Main(args);
		}
#endif

		private static void StartConsoleMode()
		{
			builder.OnBuildStatusChange += Console.WriteLine;
			builder.OnBuildFail += () => Environment.Exit(1);
			builder.Start(RunExecutable).Wait();
		}
	}
}
