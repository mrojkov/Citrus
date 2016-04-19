using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProtoBuf;
using Lime;

namespace EmptyProject
{
	[ProtoContract]
	public class ApplicationData
	{
		[ProtoMember(1)]
		public float MusicVolume = 1;

		[ProtoMember(2)]
		public float EffectsVolume = 1;

		[ProtoMember(3)]
		public float VoiceVolume = 1;

		[ProtoMember(4)]
		public GameProgress Progress = new GameProgress();
		

		public static string GetDataFilePath()
		{
			return Path.Combine(Toolbox.GetDataDirectory(DataDirectoryType.Documents), "AppData");
		}

		public ApplicationData()
		{
			var path = GetDataFilePath();
			if (File.Exists(path)) {
				Lime.Serialization.ReadObjectFromFile<ApplicationData>(path, this);
			}

			AudioSystem.SetGroupVolume(AudioChannelGroup.Music, MusicVolume);
			AudioSystem.SetGroupVolume(AudioChannelGroup.Effects, EffectsVolume);
			AudioSystem.SetGroupVolume(AudioChannelGroup.Voice, VoiceVolume);
		}

		public void Save()
		{
			EffectsVolume = AudioSystem.GetGroupVolume(AudioChannelGroup.Effects);
			MusicVolume = AudioSystem.GetGroupVolume(AudioChannelGroup.Music);
			VoiceVolume = AudioSystem.GetGroupVolume(AudioChannelGroup.Voice);

			try {
				Lime.Serialization.WriteObjectToFile(GetDataFilePath(), this);
			} catch (System.Exception e) {
				Logger.Write("Save failed: " + e.Message);
			}
		}

	}
}
