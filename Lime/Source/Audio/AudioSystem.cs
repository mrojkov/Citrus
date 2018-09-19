namespace Lime
{
	public static class AudioSystem
	{
		static readonly float[] groupVolumes = { 1, 1, 1 };
	
		public static void Initialize(ApplicationOptions options)
		{
			PlatformAudioSystem.Initialize(options);
		}

		public static void Terminate()
		{
			PlatformAudioSystem.Terminate();
		}

		public static bool Active
		{
			get { return PlatformAudioSystem.Active; }
			set { PlatformAudioSystem.Active = value; }
		}

		public static float GetGroupVolume(AudioChannelGroup group)
		{
			return groupVolumes[(int)group];
		}

		public static float SetGroupVolume(AudioChannelGroup group, float value)
		{
			float oldVolume = groupVolumes[(int)group];
			value = Mathf.Clamp(value, 0, 1);
			groupVolumes[(int)group] = value;
			PlatformAudioSystem.SetGroupVolume(group, value);
			return oldVolume;
		}

		public static void PauseGroup(AudioChannelGroup group)
		{
			PlatformAudioSystem.PauseGroup(group);
		}

		public static void ResumeGroup(AudioChannelGroup group)
		{
			PlatformAudioSystem.ResumeGroup(group);
		}

		public static void PauseAll()
		{
			PlatformAudioSystem.PauseAll();
		}

		public static void ResumeAll()
		{
			PlatformAudioSystem.ResumeAll();
		}

		public static void StopAll()
		{
			PlatformAudioSystem.StopAll();
		}

		public static void StopGroup(AudioChannelGroup group, float fadeoutTime = 0)
		{
			PlatformAudioSystem.StopGroup(group, fadeoutTime);
		}

		public static void Update()
		{
			PlatformAudioSystem.Update();
		}

		public static Sound Play(string path, AudioChannelGroup group, bool looping = false, float priority = 0.5f, float fadeinTime = 0f, bool paused = false, float volume = 1f, float pan = 0f, float pitch = 1f)
		{
			if (group == AudioChannelGroup.Music && CommandLineArgs.NoMusic) {
				return new Sound();
			}
			return PlatformAudioSystem.Play(path, group, looping, priority, fadeinTime, paused, volume, pan, pitch);
		}

		public static void ResumeSound(Sound sound, ChannelParameters channelParameters, float fadeinTime = 0)
		{
			PlatformAudioSystem.Resume(sound, channelParameters, fadeinTime);
		}

		public static Sound PlayMusic(string path, bool looping = true, float priority = 100f, float fadeinTime = 0.5f, bool paused = false, float volume = 1f, float pan = 0f, float pitch = 1f)
		{
			return Play(path, AudioChannelGroup.Music, looping, priority, fadeinTime, paused, volume, pan, pitch);
		}

		public static Sound PlayEffect(string path, bool looping = false, float priority = 0.5f, float fadeinTime = 0f, bool paused = false, float volume = 1f, float pan = 0f, float pitch = 1f)
		{
			return Play(path, AudioChannelGroup.Effects, looping, priority, fadeinTime, paused, volume, pan, pitch);
		}
	}
}
