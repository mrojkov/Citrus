using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class SerializableSample
	{
		public string Path;

		public SerializableSample() {}

		public SerializableSample(string path)
		{
			Path = path;
		}

		[ProtoMember(1)]
		public string SerializationPath
		{
			get {
				return Serialization.ShrinkPath(Path);
			}
			set {
				Path = Serialization.ExpandPath(value);
			}
		}

		public Sound Play(AudioChannelGroup group, bool paused, bool looping = false, float priority = 0.5f)
		{
			var sound = AudioSystem.LoadSound(Path, group, looping, priority);
			if (!paused) {
				sound.Resume();
			}
			return sound;
		}

		public Sound PlayEffect(bool paused, bool looping = false, float priority = 0.5f)
		{
			return Play(AudioChannelGroup.Effects, paused, looping, priority);
		}

		public Sound PlayMusic(bool paused, bool looping = true, float priority = 100)
		{
			return Play(AudioChannelGroup.Music, paused, looping, priority);
		}
	}
}