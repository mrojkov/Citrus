using ProtoBuf;
using System.Collections.Generic;

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

		public Sound Play(AudioChannelGroup group, bool paused, float fadeinTime = 0, bool looping = false, float priority = 0.5f, float volume = 1, float pan = 0, float pitch = 1)
		{
			return AudioSystem.Play(Path, group, looping, priority, fadeinTime, paused, volume, pan, pitch);
		}
	}
}