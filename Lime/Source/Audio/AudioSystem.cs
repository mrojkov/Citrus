using System;
using System.Collections.Generic;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.Threading;

namespace Lime
{
	public static class AudioSystem
	{
		static AudioContext context;
		// static XRamExtension xram;

		static List<AudioChannel> channels = new List<AudioChannel> ();
		static float [] groupVolumes = new float [2];
		
		static Thread streamingThread;
		static volatile bool shouldTerminateThread;

		public static void Initialize (int numChannels = 16)
		{
			context = new AudioContext();
			// xram = new XRamExtension();
			for (int i = 0; i < numChannels; i++) {
				channels.Add (new AudioChannel (i));
			}
			for (int i = 0; i < groupVolumes.Length; i++) {
				groupVolumes [i] = 1;
			}
			streamingThread = new Thread (RunStreamingLoop);
			//streamingThread.IsBackground = true;
			streamingThread.Priority = ThreadPriority.Highest;
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
				lock (channels) {
					foreach (var channel in channels) {
						channel.StreamBuffers ();
					}
				}
				Thread.Sleep (1);
			}
		}

		public static void ProcessEvents ()
		{
			lock (channels) {
				foreach (var channel in channels) {
					channel.ProcessEvents ();
				}
			}
		}

		static bool active = true;
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
				if (channel.IsPlaying ()) {
					channel.Pause ();
				}
			}
		}

		static void ResumeAll ()
		{
			foreach (var channel in channels) {
				if (channel.IsPaused ()) {
					channel.Resume ();
				}
			}
		}

		static void LoadSoundToChannel (AudioChannel channel, string path, AudioChannelGroup group, bool looping, int priority)
		{
			path = System.IO.Path.ChangeExtension (path, ".wav");
			var stream = AssetsBundle.Instance.OpenFile (path);
			var decoder = new WavDecoder (stream);
			channel.PlaySound (decoder, looping);
			channel.Group = group;
			channel.Priority = priority;
			channel.Volume = 1;
			// Console.WriteLine (channel.ToString ());
		}

		public static AudioChannel LoadSound (string path, AudioChannelGroup group, bool looping = false, int priority = 0)
		{
			lock (channels) {
				channels.Sort ((a, b) => { 
					if (a.Priority != b.Priority)
						return a.Priority - b.Priority;
					if (a.InitiationTime == b.InitiationTime) {
						return a.id - b.id;
					}
					return (a.InitiationTime < b.InitiationTime) ? -1 : 1;
				});
				foreach (var channel in channels) {
					if (channel.OnStop == null && (channel.IsStopped () || channel.IsInitialState ())) {
						LoadSoundToChannel (channel, path, group, looping, priority);
						return channel;
					}
				}
				if (channels [0].Priority <= priority) {
					if (channels [0].OnStop != null) {
						channels [0].Stop ();
					}
					LoadSoundToChannel (channels [0], path, group, looping, priority);
					return channels [0];
				} else {
					return null;
				}
			}
		}

		public static AudioChannel Play (string path, AudioChannelGroup group, bool looping = false, int priority = 0)
		{
			var channel = LoadSound (path, group, looping, priority);
			if (channel != null) {
				channel.Resume ();
			}
			return channel;
		}

		public static AudioChannel PlayMusic (string path, bool looping = true, int priority = 100)
		{
			return Play (path, AudioChannelGroup.Music, looping, priority);
		}

		public static AudioChannel PlayEffect (string path, bool looping = false, int priority = 0)
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