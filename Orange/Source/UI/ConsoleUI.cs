using System;
using System.Collections.Generic;
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
			base.Initialize();
#if MAC
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#else
			AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
#endif
			var args = System.Environment.GetCommandLineArgs();
			CreateMenuItems();
			if (args.Length < 3) {
				WriteHelpAndExit();
			}
			OpenWorkspace(args);
			RunCommand(Toolbox.GetCommandLineArg("--command"));
		}

		private static void OpenWorkspace(string[] args)
		{
			var projectFile = args[1];
			if (!System.IO.File.Exists(projectFile)) {
				throw new Lime.Exception("Project file '{0}' does not exist", projectFile);
			}
			The.Workspace.Open(projectFile);
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
				var builder = new SolutionBuilder(The.Workspace.ActivePlatform, The.Workspace.CustomSolution);
				builder.SvnUpdate();
			}
			The.Workspace.AssetFiles.Rescan();
			commandObj.Action();
		}

		private static void WriteHelpAndExit()
		{
			Console.WriteLine("Orange.CLI citrus_project --platform:[desktop|ios|android|uc] --command:command [--autoupdate]");
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
			The.MenuController.CreateAssemblyMenuItems();
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

		public override bool AskChoice(string text, out bool yes)
		{
			Console.WriteLine(text + " (Y/N/C)");
			var ch = Console.Read();
			if (ch == 'Y' || ch == 'y') {
				yes = true;
				return true;
			}
			yes = false;
			if (ch == 'N' || ch == 'n') {
				return true;
			}
			return false;
		}

		public override void ShowError(string message)
		{
			Console.WriteLine(message);
		}

		public override TargetPlatform GetActivePlatform()
		{
			var subTarget = GetActiveSubTarget();
			if (subTarget != null)
				return subTarget.Platform;

			var platform = Toolbox.GetCommandLineArg("--platform");
			switch (platform) {
				case "ios":
					return TargetPlatform.iOS;
				case "android":
					return TargetPlatform.Android;
				case "desktop":
				case null:
					return TargetPlatform.Desktop;
				default:
					Console.WriteLine("Target platform must be either ios, android or desktop");
					throw new TerminateException(1);
			}
		}

		public override SubTarget GetActiveSubTarget()
		{
			var platform = Toolbox.GetCommandLineArg("--platform");
			foreach (var subTarget in The.Workspace.SubTargets) {
				if (platform == subTarget.Name) {
					return subTarget;
				}
			}
			return null;
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

		public override void ExitWithErrorIfPossible()
		{
			Environment.Exit(1);
		}

		public override IPluginUIBuilder GetPluginUIBuilder()
		{
			return new PluginUIBuilder();
		}

		public override void CreatePluginUI(IPluginUIBuilder builder)
		{
		}

		public override void DestroyPluginUI()
		{
		}

		private class PluginUIBuilder : IPluginUIBuilder
		{
			public IPluginPanel SidePanel { get; } = new PluginPanel();
		}

		private class PluginPanel : IPluginPanel
		{
			public bool Enabled { get; set; }

			public string Title { get; set; }

			public ICheckBox AddCheckBox(string label)
			{
				return new CheckBox();
			}
		}

		private class CheckBox : ICheckBox
		{
			public event EventHandler Toggled;

			public bool Active { get; set; }
		}
	}
}
