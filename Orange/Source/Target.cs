namespace Orange
{
	public class Target
	{
		public readonly string Name;
		public readonly string ProjectPath;
		public readonly bool CleanBeforeBuild;
		public readonly TargetPlatform Platform;

		public Target(string name, string projectPath, bool cleanBeforeBuild, TargetPlatform platform)
		{
			Name = name;
			ProjectPath = projectPath;
			CleanBeforeBuild = cleanBeforeBuild;
			Platform = platform;
		}
	}
}
