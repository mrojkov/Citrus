using System;
using System.IO;
using ProtoBuf;

namespace Orange
{
    [ProtoContract]
	public class AppConfig
	{
		[ProtoMember(1)]
		public string AssetsFolder = "";
		
		[ProtoMember(2)]
		public int TargetPlatform;

		[ProtoMember(3)]
		public string GameAssembly = "";

		[ProtoMember(4)]
		public string GameProto = "";

		public AppConfig () {}
		
		private static string GetConfigPath ()
		{
			var basePath = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			var configPath = Path.Combine (basePath, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".config");
			return configPath;
		}
		
		public static AppConfig Load ()
		{
			try {
				using (FileStream stream = new FileStream (GetConfigPath (), FileMode.Open, FileAccess.Read, FileShare.None)) {
					return ProtoBuf.Serializer.Deserialize<AppConfig> (stream);
				}
			}
			catch {
				return new AppConfig ();
			}
		}

		public static void Save (AppConfig config)
		{
			using (FileStream stream = new FileStream (GetConfigPath (), FileMode.Create, FileAccess.Write, FileShare.None)) {
				ProtoBuf.Serializer.Serialize (stream, config);
			}
		}
	}
}

