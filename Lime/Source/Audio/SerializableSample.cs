using Yuzu;

namespace Lime
{
	public class SerializableSample
	{
		public string Path;

		public SerializableSample() {}

		public SerializableSample(string path)
		{
			SerializationPath = path;
		}

		[YuzuMember]
		public string SerializationPath
		{
			get => InternalPersistence.Current?.ShrinkPath(Path) ?? Path;
			set => Path = InternalPersistence.Current?.ExpandPath(value) ?? value;
		}

		public Sound Play(AudioChannelGroup group, bool paused, float fadeinTime = 0, bool looping = false, float priority = 0.5f, float volume = 1, float pan = 0, float pitch = 1)
		{
			return AudioSystem.Play(Path, group, looping, priority, fadeinTime, paused, volume, pan, pitch);
		}
	}
}
