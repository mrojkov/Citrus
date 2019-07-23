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
		public Vector2 ClientSize;

		[YuzuMember]
		public Vector2 ClientPosition;

		/// <summary>
		/// Asset cache mode that will be used  on workspace load
		/// </summary>
		[YuzuOptional]
		public AssetCacheMode AssetCacheMode = AssetCacheMode.Local | AssetCacheMode.Remote;

		/// <summary>
		/// Path to local asset cache that will be used on load
		/// </summary>
		[YuzuOptional]
		public string LocalAssetCachePath = Path.Combine(".orange", "Cache");

		[YuzuOptional]
		public bool BenchmarkEnabled;

		[YuzuOptional]
		public bool BundlePickerVisible = false;

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
