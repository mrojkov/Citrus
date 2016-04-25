namespace Orange
{
	public class SubTarget
	{
		public readonly string Name;
		public readonly string ProjectPath;
		public readonly bool CleanBeforeBuild;
		public readonly TargetPlatform Platform;

		public SubTarget(string name, string projectPath, bool cleanBeforeBuild, TargetPlatform platform)
		{
			Name = name;
			ProjectPath = projectPath;
			CleanBeforeBuild = cleanBeforeBuild;
			Platform = platform;
		}
	}
}
