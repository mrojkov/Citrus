using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class SerializableSample
	{
		public string Path;

		public SerializableSample () {}

		public SerializableSample (string path)
		{
			Path = path;
		}

		[ProtoMember (1)]
		public string SerializationPath
		{
			get {
				return Serialization.ShrinkPath (Path);
			}
			set {
				Path = Serialization.ExpandPath (value);
			}
		}

		public Sound Play (AudioChannelGroup group, bool paused, bool looping = false, int priority = 0)
		{
			var sound = AudioSystem.LoadSound (Path, group, looping, priority);
			if (!paused) {
				sound.Resume ();
			}
			return sound;
		}

		public Sound PlayEffect (bool paused, bool looping = false, int priority = 0)
		{
			return Play (AudioChannelGroup.Effects, paused, looping, priority);
		}

		public Sound PlayMusic (bool paused, bool looping = true, int priority = 100)
		{
			return Play (AudioChannelGroup.Music, paused, looping, priority);
		}
	}
}