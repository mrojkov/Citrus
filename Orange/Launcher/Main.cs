using System;
using System.IO;
using System.Threading.Tasks;
#if WIN
using System.Windows.Forms;
#elif MAC
using AppKit;
#endif

namespace Launcher
{
	internal static class MainClass
	{
		private static CommonBuilder builder;

		[STAThread]
		public static void Main(string[] args)
		{
			Args = new CommandLineArguments(args);
			if (!Args.AreArgumentsValid) {
				Console.WriteLine("Invalid arguments. Use -help to list possible arguments.");
				return;
			}
			if (Args.ShowHelp) {
				ShowHelp();
				return;
			}

			builder = new Builder {
				SolutionPath = Args.SolutionPath,
				ExecutablePath = Args.ExecutablePath
			};

			if (Args.ConsoleMode) {
				StartConsoleMode();
			}
			else {
				StartUIMode(args);
			}
		}

		private static CommandLineArguments Args { get; set; }

		private static void ShowHelp()
		{
			Console.WriteLine("-help: show this text.");
			Console.WriteLine("-console: console mode.");
			Console.WriteLine("-justbuild: build project without running executable.");
			Console.WriteLine("-build: project path, default: \"Orange/Orange.%Platform%.sln\".");
			Console.WriteLine("-run: executable path, default: \"Orange/bin/%Platform%/Release/%PlatformExecutable%\".");
		}

		private static bool RunExecutable => !Args.JustBuild;

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
			builder.Start(RunExecutable);
		}
	}
}