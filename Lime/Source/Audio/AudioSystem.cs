using System;
using System.Collections.Generic;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.Threading;
using System.IO;
using ProtoBuf;

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

	public static class AudioSystem
	{
		static AudioContext context;
		// static XRamExtension xram;
		static SampleCache soundCache = new SampleCache();

		static List<AudioChannel> channels = new List<AudioChannel>();
		static float[] groupVolumes = new float[2] {1, 1};

		static Thread streamingThread;
		static volatile bool shouldTerminateThread;
		static bool active = true;

		public static void Initialize(int numChannels = 16, string[] args = null)
		{
			bool silent = args != null && Array.IndexOf(args, "--Silent") >= 0;
			if (!silent) {
				context = new AudioContext();
			}
			if (!HasError()) {
				// xram = new XRamExtension();
				for (int i = 0; i < numChannels; i++) {
					channels.Add(new AudioChannel(i));
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
			if (context != null) {
				context.Dispose();
			}
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
				Thread.Sleep(0);
			}
		}

		public static bool Active
		{
			get { return active; }
			set
			{
				if (active != value) {
					active = value;
#if !iOS
					if (active)
						ResumeAll();
					else
						PauseAll();
#endif
				}
			}
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
			if (context != null) {
				context.MakeCurrent();
			}
			foreach (var channel in channels) {
				if (channel.Group == group && channel.State == ALSourceState.Paused) {
					channel.Resume();
				}
			}
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
			if (context != null) {
				context.MakeCurrent();
			}
			foreach (var channel in channels) {
				if (channel.State == ALSourceState.Paused) {
					channel.Resume();
				}
			}
		}

		static Sound LoadSoundToChannel(AudioChannel channel, string path, AudioChannelGroup group, bool looping, float priority)
		{
			IAudioDecoder decoder = null;
			path += ".sound";
			if (PackedAssetsBundle.Instance.FileExists(path)) {
				decoder = AudioDecoderFactory.CreateDecoder(soundCache.OpenStream(path));
			} else {
				Console.WriteLine("Missing audio file '{0}'", path);
				return new Sound();
			}
			var sound = channel.Play(decoder, looping);
			channel.Group = group;
			channel.Priority = priority;
			channel.Volume = 1;
			channel.Pitch = 1;
			return sound;
		}

		static AudioChannel AllocateChannel(float priority)
		{
			var channels = AudioSystem.channels.ToArray();
			Array.Sort(channels, (a, b) => {
				if (a.Priority != b.Priority)
					return Mathf.Sign(a.Priority - b.Priority);
				if (a.StartupTime == b.StartupTime) {
					return a.Id - b.Id;
				}
				return (a.StartupTime < b.StartupTime) ? -1 : 1;
			});
			foreach (var channel in channels) {
				if (!channel.Streaming) {
					var state = channel.State;
					if (state == ALSourceState.Stopped || state == ALSourceState.Initial) {
						return channel;
					}
				}
			}
			if (channels.Length > 0 && channels[0].Priority <= priority) {
				channels[0].Stop();
				return channels[0];
			} else {
				return null;
			}
		}

		public static void StopGroup(AudioChannelGroup group, float fadeoutTime = 0)
		{
			foreach (var channel in channels) {
				if (channel.Group == group) {
					channel.Stop(fadeoutTime);
				}
			}
		}

		public static Sound LoadSound(string path, AudioChannelGroup group, bool looping = false, float priority = 0.5f)
		{
			var channel = AllocateChannel(priority);
			if (channel != null) {
				return LoadSoundToChannel(channel, path, group, looping, priority);
			}
			return new Sound();
		}

		public static Sound LoadEffect(string path, bool looping = false, float priority = 0.5f)
		{
			return LoadSound(path, AudioChannelGroup.Effects, looping, priority);
		}

		public static Sound LoadMusic(string path, bool looping = false, float priority = 100f)
		{
			return LoadSound(path, AudioChannelGroup.Music, looping, priority);
		}

		public static Sound Play(string path, AudioChannelGroup group, bool looping = false, float priority = 0.5f, float fadeinTime = 0)
		{
			var sound = LoadSound(path, group, looping, priority);
			sound.Resume(fadeinTime);
			return sound;
		}

		public static Sound PlayMusic(string path, bool looping = true, float priority = 100f, float fadeinTime = 0.5f)
		{
			return Play(path, AudioChannelGroup.Music, looping, priority, fadeinTime);
		}

		public static Sound PlayEffect(string path, bool looping = false, float priority = 0.5f, float fadeinTime = 0)
		{
			return Play(path, AudioChannelGroup.Effects, looping, priority, fadeinTime);
		}

		public static bool HasError()
		{
			return AL.GetError() != ALError.NoError;
		}

		public static void CheckError()
		{
			var error = AL.GetError();
			if (error != ALError.NoError) {
				throw new Exception("OpenAL error: " + AL.GetErrorString(error));
			}
		}
	}
}