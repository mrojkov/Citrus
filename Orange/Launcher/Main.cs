using System;
using McMaster.Extensions.CommandLineUtils;
using System.Linq;
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

		private static CommonBuilder builder;

		[STAThread]
		public static int Main(string[] args)
		{
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
			var optionRunProjectPath = cli.Option<string>("-r --run <EXECUTABLE_PATH>", "Executable path, default: \"Orange/bin/%Platform%/Release/%PlatformExecutable%\".", CommandOptionType.SingleValue);
			var optionRunArgs = cli.Option<string>("-a --runargs <ARGUMENTS>", "Args to pass to executable.", CommandOptionType.SingleValue);

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
				builder = new Builder {
					NeedRunExecutable = !optionJustBuild.HasValue(),
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
			builder.Start(RunExecutable);
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
#endif

		private static void StartConsoleMode()
		{
			builder.OnBuildStatusChange += Console.WriteLine;
			builder.OnBuildFail += () => Environment.Exit(1);
			builder.Start().Wait();
		}
	}
}
