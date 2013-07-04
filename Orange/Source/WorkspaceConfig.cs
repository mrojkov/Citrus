using System;
using System.IO;
using ProtoBuf;

namespace Orange
{
    [ProtoContract]
	public class WorkspaceConfig
	{
		[ProtoMember(1)]
		public string CitrusProject = "";

		[ProtoMember(2)]
		public int TargetPlatform;

		[ProtoMember(5)]
		public int Action;

		public WorkspaceConfig() {}

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
					return ProtoBuf.Serializer.Deserialize<WorkspaceConfig>(stream);
				}
			}
			catch {
				return new WorkspaceConfig();
			}
		}

		public static void Save(WorkspaceConfig config)
		{
			using(FileStream stream = new FileStream(GetConfigPath(), FileMode.Create, FileAccess.Write, FileShare.None)) {
				ProtoBuf.Serializer.Serialize(stream, config);
			}
		}
	}
}
