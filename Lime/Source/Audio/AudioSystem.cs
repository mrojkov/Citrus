using System;
using System.Collections.Generic;
using System.Linq;
#if OPENAL
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
#endif
using System.Threading;
using System.IO;
using ProtoBuf;
using System.ComponentModel;

namespace Lime
{
	[ProtoContract]
	public enum AudioChannelGroup
	{
		[ProtoEnum]
		Effects,
		[ProtoEnum]
		Music
	}

	public static partial class AudioSystem
	{
#if OPENAL
		static AudioContext context;
#endif
		static readonly List<AudioChannel> channels = new List<AudioChannel>();
		static readonly float[] groupVolumes = new float[2] {1, 1};

		static Thread streamingThread;
		static volatile bool shouldTerminateThread;
		static bool active = true;
		static public bool SilentMode { get; private set; }

		public static void Initialize(int numStereoChannels = 4, int numMonoChannels = 12)
		{
#if OPENAL
#if !iOS
			SilentMode = Application.CheckCommandLineArg("--Silent");
			if (!SilentMode) {
				context = new AudioContext();
			}
#else
			context = new AudioContext();
#endif
#endif
			if (!HasError()) {
				// iOS dislike to mix stereo and mono buffers on one audio source, so separate them
				for (int i = 0; i < numStereoChannels; i++) {
					channels.Add(new AudioChannel(i, AudioFormat.Stereo16));
				}
				for (int i = 0; i < numMonoChannels; i++) {
					channels.Add(new AudioChannel(i, AudioFormat.Mono16));
				}
			}
			streamingThread = new Thread(RunStreamingLoop);
			streamingThread.IsBackground = true;
			streamingThread.Start();
		}

		public static void Terminate()
		{
			shouldTerminateThread = true;
			streamingThread.Join();
			foreach (var channel in channels) {
				channel.Dispose();
			}
#if OPENAL
			if (context != null) {
				context.Dispose();
			}
#endif
		}

		private static long tickCount;

		private static long GetTimeDelta()
		{
			long delta = (System.DateTime.Now.Ticks / 10000L) - tickCount;
			if (tickCount == 0) {
				tickCount = delta;
				delta = 0;
			} else {
				tickCount += delta;
			}
			return delta;
		}

		static void RunStreamingLoop()
		{
			while (!shouldTerminateThread) {
				float delta = GetTimeDelta() * 0.001f;
				foreach (var channel in channels) {
					channel.Update(delta);
				}
				Thread.Sleep(10);
			}
		}

		public static bool Active
		{
			get { return active; }
			set { SetActive(value); }
		}

		private static void SetActive(bool value)
		{
			if (active == value) {
				return;
			}
			active = value;
#if !iOS
			if (active) {
				ResumeAll();
			} else {
				PauseAll();
			}
#endif
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
			foreach (var channel in channels) {
				if (channel.Group == group) {
					channel.Volume = channel.Volume;
				}
			}
			return oldVolume;
		}

		public static void PauseGroup(AudioChannelGroup group)
		{
			foreach (var channel in channels) {
				if (channel.Group == group && channel.State == ALSourceState.Playing) {
					channel.Pause();
				}
			}
		}

		public static void ResumeGroup(AudioChannelGroup group)
		{
#if OPENAL
			if (context != null) {
				context.MakeCurrent();
			}
			foreach (var channel in channels) {
				if (channel.Group == group && channel.State == ALSourceState.Paused) {
					channel.Resume();
				}
			}
#endif
		}

		public static void PauseAll()
		{
			foreach (var channel in channels) {
				if (channel.State == ALSourceState.Playing) {
					channel.Pause();
				}
			}
		}

		public static void ResumeAll()
		{
#if OPENAL
			if (context != null) {
				context.MakeCurrent();
			}
			foreach (var channel in channels) {
				if (channel.State == ALSourceState.Paused) {
					channel.Resume();
				}
			}
#endif
		}

		delegate AudioChannel ChannelSelector(AudioFormat format);

		static readonly AudioCache cache = new AudioCache();

		private static Sound LoadSoundToChannel(ChannelSelector channelSelector, string path, bool looping, bool paused, float fadeinTime)
		{
			if (SilentMode) {
				return new Sound();
			}
			path += ".sound";
			var sound = new Sound();
			var stream = cache.OpenStream(path);
			var decoder = AudioDecoderFactory.CreateDecoder(stream);
			var channel = channelSelector(decoder.GetFormat());
			if (channel == null) {
				return sound;
			}
			channel.SamplePath = path;
			channel.Play(sound, decoder, looping, paused, fadeinTime);
			return sound;
		}

		static AudioChannel AllocateChannel(float priority, AudioFormat format)
		{
			var channels = AudioSystem.channels.Where(c => c.AudioFormat == format).ToArray();
			Array.Sort(channels, (a, b) => {
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
				if (channel.Streaming) {
					continue;
				}
				var state = channel.State;
				if (state == ALSourceState.Stopped || state == ALSourceState.Initial) {
					return channel;
				}
			}
			// Trying to stop first channel in order of priority
			foreach (var channel in channels) {
				if (channel.Priority <= priority) {
					channel.Stop();
					return channel;
				}
			}
			return null;
		}

		public static void StopGroup(AudioChannelGroup group, float fadeoutTime = 0)
		{
			foreach (var channel in channels) {
				if (channel.Group == group) {
					channel.Stop(fadeoutTime);
				}
			}
		}

		public static Sound Play(string path, AudioChannelGroup group, bool looping = false, float priority = 0.5f, float fadeinTime = 0f, bool paused = false, float volume = 1f, float pan = 0f, float pitch = 1f)
		{
			ChannelSelector channelSelector = (format) => {
				var channel = AllocateChannel(priority, format);
				if (channel != null) {
					if (channel.Sound != null) {
						channel.Sound.Channel = NullAudioChannel.Instance;
					}
					channel.Group = group;
					channel.Priority = priority;
					channel.Volume = volume;
					channel.Pitch = pitch;
					channel.Pan = 0;
				}
				return channel;
			};
			var sound = LoadSoundToChannel(channelSelector, path, looping, paused, fadeinTime);
			return sound;
		}

		public static Sound PlayMusic(string path, bool looping = true, float priority = 100f, float fadeinTime = 0.5f, bool paused = false, float volume = 1f, float pan = 0f, float pitch = 1f)
		{
			return Play(path, AudioChannelGroup.Music, looping, priority, fadeinTime, paused, volume, pan, pitch);
		}

		public static Sound PlayEffect(string path, bool looping = false, float priority = 0.5f, float fadeinTime = 0f, bool paused = false, float volume = 1f, float pan = 0f, float pitch = 1f)
		{
			return Play(path, AudioChannelGroup.Effects, looping, priority, fadeinTime, paused, volume, pan, pitch);
		}

		public static bool HasError()
		{
#if OPENAL
			return AL.GetError() != ALError.NoError;
#else
			return false;
#endif
		}
	}
}