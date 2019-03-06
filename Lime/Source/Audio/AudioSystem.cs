using System;
using System.Collections.Generic;

namespace Lime
{
	public class PlayParameters : IDisposable
	{
		public string Path;
		public IAudioDecoder Decoder;
		public AudioChannelGroup Group;
		public float Pan = 0f;
		public float Volume = 1f;
		public float Pitch = 1f;
		public float Priority = 0.5f;
		public bool Looping = false;
		public bool Paused = false;

		public void Dispose()
		{
			Decoder?.Dispose();
		}
	}

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

		public static event Action<string> AudioMissing
		{
			add { PlatformAudioSystem.AudioMissing += value; }
			remove { PlatformAudioSystem.AudioMissing -= value; }
		}

		public static IEnumerable<IAudioChannel> Channels => PlatformAudioSystem.Channels;

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
			return PlatformAudioSystem.Play(new PlayParameters {
				Path = path,
				Group = group,
				Looping = looping,
				Priority = priority,
				Paused = paused,
				Volume = volume,
				Pan = pan,
				Pitch = pitch
			},
			fadeinTime);
		}

		public static Sound Play(PlayParameters parameters, float fadeinTime = 0f)
		{
			return PlatformAudioSystem.Play(parameters, fadeinTime);
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
