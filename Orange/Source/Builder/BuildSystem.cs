using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orange.Source
{
	public enum BuildAction
	{
		Clean,
		Build
	}

	abstract class BuildSystem
	{
		private List<string> arguments;

		public TargetPlatform Platform { get; }
		public string SolutionPath { get; }
		public string Configuration { get; }

		protected string Args => string.Join(" ", arguments);

		public string BinariesDirectory => Path.Combine(
			Path.GetDirectoryName(SolutionPath), "bin", Configuration);


		public BuildSystem(TargetPlatform platform, string solutionPath, string configuration)
		{
			arguments = new List<string>();
			Platform = platform;
			SolutionPath = solutionPath ?? The.Workspace.GetSolutionFilePath();
			Configuration = configuration ?? "Release";
		}

		public int Execute(BuildAction buildAction, StringBuilder output = null)
		{
			arguments.Clear();
			switch (buildAction) {
				case BuildAction.Clean: {
					PrepareForClean();
					return Execute(output);
				}
				case BuildAction.Build: {
					PrepareForBuild();
					return Execute(output);
				}
				default: {
					throw new InvalidOperationException($"Unknown {nameof(buildAction)}: {buildAction}");
				}
			}
		}

		protected abstract void DecorateBuild();
		protected abstract void DecorateClean();
		protected abstract void DecorateConfiguration();
		protected abstract int Execute(StringBuilder output);

		protected void AddArgument(string argument)
		{
			arguments.Add(argument);
		}

		private void PrepareForBuild()
		{
			DecorateBuild();
			DecorateConfiguration();
		}

		private void PrepareForClean()
		{
			DecorateClean();
			DecorateConfiguration();
		}
	}
}
