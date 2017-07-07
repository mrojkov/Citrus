using System.Linq;

namespace Launcher
{
	internal class CommandLineArguments
	{
		public CommandLineArguments(string[] args)
		{
			AreArgumentsValid =
				args.All(arg => arg.StartsWith("-")) &&
				args
				    // Mac-specific argument
				    .Where(arg => !arg.StartsWith("-psn"))
				    .Select(line => line.TrimStart('-'))
				    .All(ReadArgument);
		}

		private bool ReadArgument(string arg)
		{
			var parts = arg.Split(':');
			var first = parts[0].ToLower();
			switch (first) {
				case "help":
					ShowHelp = true;
					return true;
				case "console":
					ConsoleMode = true;
					return true;
				case "justbuild":
					JustBuild = true;
					return true;
				case "build":
					SolutionPath = parts[1];
					return true;
				case "run":
					ExecutablePath = parts[1];
					return true;
				case "assemblies":
					Assemblies = parts[1].Split(';');
					return true;
				default:
					return false;
			}
		}

		public bool AreArgumentsValid { get; private set; }
		public bool ConsoleMode { get; private set; }
		public bool JustBuild { get; private set; }
		public bool ShowHelp { get; private set; }
		public string SolutionPath { get; private set; }
		public string ExecutablePath { get; private set; }
		public string[] Assemblies { get; private set; }
	}
}
