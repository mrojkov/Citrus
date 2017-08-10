using System;
using System.IO;
using Yuzu;

namespace Orange
{
	public class WorkspaceConfig
	{
		[YuzuMember]
		public string CitrusProject = "";

		[YuzuMember]
		public int ActiveTargetIndex;

		[YuzuMember]
		public bool UpdateBeforeBuild;

		private static string GetConfigPath()
		{
			var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var configPath = Path.Combine(basePath, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".config");
			return configPath;
		}

		public static WorkspaceConfig Load()
		{
			try {
				using(FileStream stream = new FileStream(GetConfigPath(), FileMode.Open, FileAccess.Read, FileShare.None)) {
					var jd = new Yuzu.Json.JsonDeserializer();
					return (WorkspaceConfig)jd.FromStream(new WorkspaceConfig(), stream);
				}
			}
			catch {
				return new WorkspaceConfig();
			}
		}

		public static void Save(WorkspaceConfig config)
		{
			using(FileStream stream = new FileStream(GetConfigPath(), FileMode.Create, FileAccess.Write, FileShare.None)) {
				var js = new Yuzu.Json.JsonSerializer();
				js.ToStream(config, stream);
			}
		}
	}
}
