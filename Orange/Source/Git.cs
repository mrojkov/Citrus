using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Orange
{
	public static class Git
	{
		private static readonly Regex gitConflictRegex = new Regex("^<<<<<<<.*?^=======.*?^>>>>>>>",
			RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.Singleline);

		public static bool Exec(string workingDirectory, string gitArgs, StringBuilder output = null)
		{
			return Process.Start("git", gitArgs, workingDirectory, Process.Options.RedirectErrors | Process.Options.RedirectOutput, output) == 0;
		}

		public static string GetCurrentBranch(string gitDir)
		{
			var sb = new StringBuilder();
			if (Exec(gitDir, "rev-parse --abbrev-ref HEAD", sb)) {
				return sb.ToString().Trim();
			} else {
				return null;
			}
		}

		public static void ForceUpdate(string gitDir)
		{
			new Thread(() => {
				var branch = GetCurrentBranch(gitDir);
				Exec(gitDir, $"fetch origin");
				Exec(gitDir, $"reset --hard origin/{branch}");
			}).Start();
		}

		public static void Update(string gitDir)
		{
			new Thread(() => {
				var branch = GetCurrentBranch(gitDir);
				Exec(gitDir, $"pull --ff-only origin {branch}");
			}).Start();
		}

		public static bool HasGitConflicts(string path) =>
			gitConflictRegex.IsMatch(System.IO.File.ReadAllText(path));
	}
}
