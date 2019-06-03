using System;
using System.IO;
using Yuzu;
using Lime;

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

		[YuzuMember]
		public Vector2 ClientSize;

		[YuzuMember]
		public Vector2 ClientPosition;

		[YuzuOptional]
		public AssetCacheMode AssetCacheMode = AssetCacheMode.Local | AssetCacheMode.Remote;

		[YuzuOptional]
		public string AssetCacheLocalPath = ".orange/Cache";

		[YuzuOptional]
		public bool BenchmarkEnabled;

		public static string GetDataPath()
		{
			var name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
			return Lime.Environment.GetDataDirectory("Game Forest", name, "1.0");
		}

		private static string GetConfigPath()
		{
			var configPath = Path.Combine(GetDataPath(), ".config");
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
