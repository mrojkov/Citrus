#if UNITY
using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public static class PlatformAudioSystem
	{
		static readonly List<AudioChannel> channels = new List<AudioChannel>();

		public static bool Active;

		public static void Initialize(ApplicationOptions options)
		{
			if (options.DecodeAudioInSeparateThread) {
				throw new Lime.Exception("Unity3D doesn't support setting audio volume in separate thread. Set DecodeAudioInSeparateThread = false and use AudioSystem.Update()");
			}
			for (int i = 0; i < options.NumStereoChannels + options.NumMonoChannels; i++) {
				channels.Add(new AudioChannel(i));
			}
		}

		public static void Terminate()
		{
			foreach (var channel in channels) {
				channel.Dispose();
			}
			channels.Clear();
		}
	
		public static void Update()
		{
			foreach (var channel in channels) {
				channel.Update(UnityEngine.Time.deltaTime);
			}
		}

		public static void SetGroupVolume(AudioChannelGroup group, float value)
		{
			foreach (var channel in channels) {
				if (channel.Group == group) {
					channel.Volume = channel.Volume;
				}
			}
		}

		public static void PauseGroup(AudioChannelGroup group)
		{
			foreach (var channel in channels) {
				if (channel.Group == group && channel.State == AudioChannelState.Playing) {
					channel.Pause();
				}
			}
		}

		public static void ResumeGroup(AudioChannelGroup group)
		{
			foreach (var channel in channels) {
				if (channel.Group == group && channel.State == AudioChannelState.Paused) {
					channel.Resume();
				}
			}
		}


		public static void PauseAll()
		{
			foreach (var channel in channels) {
				if (channel.State == AudioChannelState.Playing) {
					channel.Pause();
				}
			}
		}

		public static void ResumeAll()
		{
			foreach (var channel in channels) {
				if (channel.State == AudioChannelState.Paused) {
					channel.Resume();
				}
			}
		}

		public static void BumpAll()
		{
			foreach (var channel in channels) {
				channel.Bump();
			}
		}

		public static void StopGroup(AudioChannelGroup group, float fadeoutTime)
		{
			foreach (var channel in channels) {
				if (channel.Group == group) {
					channel.Stop(fadeoutTime);
				}
			}
		}

		delegate AudioChannel ChannelSelector();
		
		private static Sound LoadSoundToChannel(ChannelSelector channelSelector, string path, bool looping, bool paused, float fadeinTime)
		{
			var sound = new Sound();
			var channel = channelSelector();
			if (channel == null) {
				return sound;
			}
			var r = UnityEngine.Resources.Load(path);
			var clip = r as UnityEngine.AudioClip;
			if (clip == null) {
				return sound;
			}
			channel.SamplePath = path;
			channel.Play(sound, clip, looping, paused, fadeinTime);
			return sound;
		}

		private static AudioChannel AllocateChannel(float priority)
		{
			var channels = PlatformAudioSystem.channels.ToList();
			channels.Sort((a, b) => {
				if (a.Priority != b.Priority) {
					return Mathf.Sign(a.Priority - b.Priority);
				}
				if (a.StartupTime == b.StartupTime) {
					return a.Id - b.Id;
				}
				return (a.StartupTime < b.StartupTime) ? -1 : 1;
			});
			// Looking for stopped channels
			foreach (var channel in channels) {
				var state = channel.State;
				if (state == AudioChannelState.Stopped || state == AudioChannelState.Initial) {
					return channel;
				}
			}
			// Trying to stop first channel in order of priority
			foreach (var channel in channels) {
				if (channel.Priority <= priority) {
					channel.Stop();
					if (channel.State == AudioChannelState.Stopped) {
						return channel;
					}
				}
			}
			return null;
		}

		public static Sound Play(string path, AudioChannelGroup group, bool looping = false, float priority = 0.5f, float fadeinTime = 0f, bool paused = false, float volume = 1f, float pan = 0f, float pitch = 1f)
		{
			ChannelSelector channelSelector = () => {
				var channel = AllocateChannel(priority);
				if (channel != null) {
					if (channel.Sound != null) {
						channel.Sound.Channel = NullAudioChannel.Instance;
					}
					channel.Group = group;
					channel.Priority = priority;
					channel.Volume = volume;
					channel.Pitch = pitch;
					channel.Pan = pan;
				}
				return channel;
			};
			var sound = LoadSoundToChannel(channelSelector, path, looping, paused, fadeinTime);
			return sound;
		}
	}
}
#endif
