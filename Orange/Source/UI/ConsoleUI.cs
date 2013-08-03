using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	public class TerminateException : Exception
	{
		public int Code;

		public TerminateException(int code)
		{
			Code = code;
		}
	}

	public class ConsoleUI : UserInterface
	{
		public override void Initialize()
		{
			Gtk.Application.Init();
#if MAC
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#else
			AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
#endif
			var args = System.Environment.GetCommandLineArgs();
			if (args.Length < 3) {
				WriteHelpAndExit();
			}
			CreateMenuItems();
			The.Workspace.Load();
			RunCommand(Toolbox.GetCommandLineArg("--command"));
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var te = e.ExceptionObject as TerminateException;
			if (te != null) {
				System.Environment.Exit(te.Code);
			}
		}

		void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
		{
			var te = e.Exception as TerminateException;
			if (te != null) {
				System.Environment.Exit(te.Code);
			}
		}

		private void RunCommand(string command)
		{
			if (command == null) {
				WriteHelpAndExit();
			}
			var commands = The.MenuController.GetVisibleAndSortedItems();
			var commandObj = commands.Find(i => i.Label == command);
			if (commandObj == null) {
				Console.WriteLine("Unknown command: '{0}'", command); 
				WriteHelpAndExit();
			}
			if (DoesNeedSvnUpdate()) {
				SolutionBuilder.SvnUpdate();
			}
			The.Workspace.AssetFiles.Rescan();
			commandObj.Action();
		}

		private static void WriteHelpAndExit()
		{
			Console.WriteLine("Orange --console citrus_project --platform:[ios|desktop] --command:command [--autoupdate]");
			var commands = The.MenuController.GetVisibleAndSortedItems();
			if (commands.Count > 0) {
				Console.WriteLine("Available commands are:");
				foreach (var item in commands) {
					Console.WriteLine("\"" + item.Label + "\"");
				}
			}
			throw new TerminateException(1);
		}

		public override bool DoesNeedSvnUpdate()
		{
			return Toolbox.GetCommandLineFlag("--autoupdate");
		}

		private static void CreateMenuItems()
		{
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			The.MenuController.CreateAssemblyMenuItems(assembly);
		}

		public override string GetConsoleOutput()
		{
			throw new NotImplementedException();
		}

		public override bool AskConfirmation(string text)
		{
			Console.WriteLine(text + " (Y/N)");
			var ch = Console.Read();
			if (ch == 'Y' || ch == 'y') {
				return true;
			}
			return false;
		}

		public override TargetPlatform GetActivePlatform()
		{
			var platform = Toolbox.GetCommandLineArg("--platform");
			if (platform == "ios") {
				return TargetPlatform.iOS;
			} else if (platform == "desktop") {
				return TargetPlatform.Desktop;
			} else if (platform == null) {
				return TargetPlatform.Desktop;
			} else {
				Console.WriteLine("Target platform must be either ios or desktop");
				throw new TerminateException(1);
			}
		}

		public override void ScrollLogToEnd()
		{
		}

		public override void OnWorkspaceOpened()
		{
		}

		public override void RefreshMenu()
		{
		}

		public override void ClearLog()
		{
		}
	}
}
