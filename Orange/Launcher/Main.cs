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
		public static void Main (string [] args)
		{
			Args = new CommandLineArguments (args);
			if (!Args.AreArgumentsValid) {
				Console.WriteLine ("Invalid arguments. Use -help to list possible arguments.");
				return;
			}
			if (Args.ShowHelp) {
				ShowHelp ();
				return;
			}
			if (Args.Assemblies != null && Args.Assemblies.Length > 0) {
				InstallAssemblies (args);
				return;
			}

			builder = new Builder {
				SolutionPath = Args.SolutionPath,
				ExecutablePath = Args.ExecutablePath
			};

			if (Args.ConsoleMode) {
				StartConsoleMode ();
			} else {
				StartUIMode (args);
			}
		}

		private static CommandLineArguments Args { get; set; }

		private static void ShowHelp ()
		{
			Console.WriteLine ("-help: show this text.");
			Console.WriteLine ("-console: console mode.");
			Console.WriteLine ("-justbuild: build project without running executable.");
			Console.WriteLine ("-build: project path, default: \"Orange/Orange.%Platform%.sln\".");
			Console.WriteLine ("-run: executable path, default: \"Orange/bin/%Platform%/Release/%PlatformExecutable%\".");
			Console.WriteLine ("-assemblies: set required project's assemblies");
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
		private static void StartUIMode (string [] args)
		{
			AppDelegate.OnFinishLaunching = (appDelegate) => {
				builder.OnBuildSuccess += () => appDelegate.InvokeOnMainThread (
					() => NSApplication.SharedApplication.Terminate (appDelegate)
				);
				builder.OnBuildStatusChange += appDelegate.MainWindowController.SetBuildStatus;
				builder.OnBuildFail += appDelegate.MainWindowController.ShowLog;
				builder.Start (RunExecutable);
			};

			NSApplication.Init ();
			NSApplication.Main (args);
		}
#endif

		private static void StartConsoleMode ()
		{
			builder.OnBuildStatusChange += Console.WriteLine;
			builder.OnBuildFail += () => Environment.Exit (1);
			builder.Start (RunExecutable);
		}

#if WIN
		private static void InstallAssemblies(string[] args)
		{
			var mainForm = new MainForm();
			Console.SetOut(mainForm.LogWriter);
			Console.SetError(mainForm.LogWriter);
			mainForm.Show();
			mainForm.ShowLog();
			new Task(InstallAssembliesTask).Start();
			Application.Run();
		}
#elif MAC
		private static void InstallAssemblies (string [] args)
		{
			AppDelegate.OnFinishLaunching = (appDelegate) => {
				appDelegate.MainWindowController.ShowLog ();
				new Task (InstallAssembliesTask).Start ();
			};

			NSApplication.Init ();
			NSApplication.Main (args);
		}
#endif

		public static void InstallAssembliesTask ()
		{
			Console.WriteLine ($"Installing assemblies...");
			System.Threading.Thread.Sleep (500);

			var isSuccessfull = true;
			try {
#if WIN
				var executablePath = Args.ExecutablePath ?? Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Orange.GUI.exe");
#elif MAC
				var executablePath = Args.ExecutablePath ?? Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "../../../Orange.GUI.app/Contents/MacOS/Orange.GUI");
				Console.WriteLine(executablePath);
#endif
				new AssembliesInstaller().Start(Args.Assemblies, executablePath);
			} catch (Exception exception) {
				isSuccessfull = false;
				Console.WriteLine(exception);
			}

			if (isSuccessfull) {
				Environment.Exit(0);
			}
		}
	}
}