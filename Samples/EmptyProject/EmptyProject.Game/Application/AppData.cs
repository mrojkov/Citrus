using System;
using System.IO;
using Lime;
using ProtoBuf;

namespace EmptyProject.Application
{
	[ProtoContract]
	public class AppData
	{
		public static AppData Instance;

		public const string Version = "0.1";

		[ProtoMember(1)]
		public float MusicVolume = 1;

		[ProtoMember(2)]
		public float EffectsVolume = 1;

		[ProtoMember(3)]
#if !iOS && !ANDROID
		public Display SimulateDisplay = DisplayInfo.Displays[0];
#else
		public Display SimulateDisplay;
#endif

		[ProtoMember(4)]
		public DeviceOrientation SimulateDeviceOrientation = DeviceOrientation.LandscapeLeft;

		[ProtoMember(5)]
		public bool IsLevelConfigsOnWebModeOn;

		public static string GetDataFilePath()
		{
			return Path.Combine(GetDataDirectory(), "AppData.dat");
		}

		public static string GetDataDirectory()
		{
			return Lime.Environment.GetDataDirectory("Game Forest", Application.ApplicationName, Version);
		}

		public static void Load()
		{
			var path = GetDataFilePath();
			Instance = null;
			if (File.Exists(path)) {
				try {
					Instance = Lime.Serialization.ReadObjectFromFile<AppData>(path);
				}
				catch (System.Exception e) {
					Console.WriteLine("Failed to load the application profile: {0}", e.Message);
				}
			}
			if (Instance == null) {
				Instance = new AppData();
			}
			AudioSystem.SetGroupVolume(AudioChannelGroup.Music, Instance.MusicVolume);
			AudioSystem.SetGroupVolume(AudioChannelGroup.Effects, Instance.EffectsVolume);
		}

		public void Save()
		{
			EffectsVolume = AudioSystem.GetGroupVolume(AudioChannelGroup.Effects);
			MusicVolume = AudioSystem.GetGroupVolume(AudioChannelGroup.Music);
			try {
				Serialization.WriteObjectToFile(GetDataFilePath(), this);
			}
			catch (System.Exception e) {
				Logger.Write("Save failed: " + e.Message);
			}
		}

	}
}