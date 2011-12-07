using System;
using System.Collections.Generic;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.Threading;
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
		static SampleCache soundCache = new SampleCache ();

		static List<AudioChannel> channels = new List<AudioChannel> ();
		static float [] groupVolumes = new float [2] {1, 1};

		static Thread streamingThread;
		static volatile bool shouldTerminateThread;
		static bool active = true;

		public static void Initialize (int numChannels = 16)
		{
			context = new AudioContext();
			if (!HasError ()) {
				// xram = new XRamExtension();
				for (int i = 0; i < numChannels; i++) {
					channels.Add (new AudioChannel (i));
				}
			}
			streamingThread = new Thread (RunStreamingLoop);
			streamingThread.IsBackground = true;
			streamingThread.Start ();
		}

		public static void Terminate ()
		{
			shouldTerminateThread = true;
			streamingThread.Join ();
			foreach (var channel in channels) {
				channel.Dispose ();
			}
			context.Dispose ();
		}

		static void RunStreamingLoop ()
		{
			while (!shouldTerminateThread) {
				foreach (var channel in channels) {
					channel.Update ();
				}
				Thread.Sleep (0);
			}
		}

		public static bool Active
		{
			get { return active; }
			set
			{
				if (active != value) {
					active = value;
					if (active)
						ResumeAll ();
					else
						PauseAll ();
				}
			}
		}

		public static float GetGroupVolume (AudioChannelGroup group)
		{
			return groupVolumes [(int)group];
		}

		public static void SetGroupVolume (AudioChannelGroup group, float value)
		{
			value = Utils.Clamp (value, 0, 1);
			groupVolumes [(int)group] = value;
			foreach (var channel in channels) {
				if (channel.Group == group) {
					channel.Volume = channel.Volume;
				}
			}
		}

		static void PauseAll ()
		{
			foreach (var channel in channels) {
				if (channel.State == ALSourceState.Playing) {
					channel.Pause ();
				}
			}
		}

		static void ResumeAll ()
		{
			foreach (var channel in channels) {
				if (channel.State == ALSourceState.Paused) {
					channel.Resume ();
				}
			}
		}

		static Sound LoadSoundToChannel (AudioChannel channel, string path, AudioChannelGroup group, bool looping, int priority)
		{
			IAudioDecoder decoder = null;
			if (AssetsBundle.Instance.FileExists (path + ".ogg")) {
				decoder = new OggDecoder (soundCache.OpenStream (path + ".ogg"));
			} else if (AssetsBundle.Instance.FileExists (path + ".wav")) {
				decoder = new WaveIMA4Decoder (soundCache.OpenStream (path + ".wav"));
			} else {
				Console.WriteLine ("Missing audio file: '{0}'", path);
				return new Sound ();
			}
			var sound = channel.Play (decoder, looping);
			channel.Group = group;
			channel.Priority = priority;
			channel.Volume = 1;
			return sound;
		}

		static AudioChannel AllocateChannel (int priority)
		{
			var channels = AudioSystem.channels.ToArray ();
			Array.Sort (channels, (a, b) => {
				if (a.Priority != b.Priority)
					return a.Priority - b.Priority;
				if (a.StartupTime == b.StartupTime) {
					return a.Id - b.Id;
				}
				return (a.StartupTime < b.StartupTime) ? -1 : 1;
			});
			foreach (var channel in channels) {
				if (channel.State == ALSourceState.Stopped || channel.State == ALSourceState.Initial) {
					return channel;
				}
			}
			if (channels.Length > 0 && channels [0].Priority <= priority) {
				channels [0].Stop ();
				return channels [0];
			} else {
				return null;
			}
		}

		public static Sound LoadSound (string path, AudioChannelGroup group, bool looping = false, int priority = 0)
		{
			var channel = AllocateChannel (priority);
			if (channel != null) {
				return LoadSoundToChannel (channel, path, group, looping, priority);
			}
			return new Sound ();
		}

		public static Sound Play (string path, AudioChannelGroup group, bool looping = false, int priority = 0)
		{
			var sound = LoadSound (path, group, looping, priority);
			sound.Resume ();
			return sound;
		}

		public static Sound PlayMusic (string path, bool looping = true, int priority = 100)
		{
			return Play (path, AudioChannelGroup.Music, looping, priority);
		}

		public static Sound PlayEffect (string path, bool looping = false, int priority = 0)
		{
			return Play (path, AudioChannelGroup.Effects, looping, priority);
		}

		public static bool HasError ()
		{
			return AL.GetError () != ALError.NoError;
		}

		public static void CheckError ()
		{
			var error = AL.GetError ();
			if (error != ALError.NoError) {
				throw new Exception ("OpenAL error: " + AL.GetErrorString (error));
			}
		}
	}
}