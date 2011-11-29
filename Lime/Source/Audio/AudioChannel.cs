using System;
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

	public class AudioChannel : IDisposable
	{
		public const int BufferSize = 1024 * 32;
		public const int NumBuffers = 4;

		public delegate void StopEvent (AudioChannel channel);
		public StopEvent OnStop;

		public bool Looping;

		internal AudioChannelGroup Group;
		internal int Priority;
		internal DateTime InitiationTime;

		internal int id;
		int source;
		int [] buffers;
		internal IAudioDecoder decoder;

		internal AudioChannel (int index)
		{
			this.id = index;
			buffers = AL.GenBuffers (NumBuffers);
			source = AL.GenSource ();
			AudioSystem.CheckError ();
		}

		public void Dispose ()
		{
			AL.SourceStop (source);
			AL.DeleteSource (source);
			AL.DeleteBuffers (buffers);
			AudioSystem.CheckError ();
		}

		internal void PlaySound (IAudioDecoder decoder, bool looping)
		{
			this.Looping = looping;
			this.decoder = decoder;
			InitiationTime = DateTime.Now;
			int queuedBuffers = 0;
			AL.SourceStop (source);
			AL.GetSource (source, ALGetSourcei.BuffersQueued, out queuedBuffers);
			if (queuedBuffers > 0) {
				AL.SourceUnqueueBuffers (source, queuedBuffers);
			}
			AudioSystem.CheckError ();
			for (int i = 0; i < NumBuffers; i++) {
				if (!StreamBuffer (buffers [i]))
					break;
				AL.SourceQueueBuffer (source, buffers [i]);
				AudioSystem.CheckError ();
			}
		}

		public void Resume ()
		{
			AL.SourcePlay (source);
			AudioSystem.CheckError ();
		}

		/// <summary>
		/// Pauses channel. Used only for pausing on application deactivation.
		/// </summary>
		internal void Pause ()
		{
			AL.SourcePause (source);
			AudioSystem.CheckError ();
		}

		float volume = 1;
		public float Volume
		{
			get { return volume; }
			set
			{
				volume = Utils.Clamp (value, 0, 1);
				float gain = volume * AudioSystem.GetGroupVolume (Group);
				AL.Source (source, ALSourcef.Gain, gain);
				AudioSystem.CheckError ();
			}
		}

		// Not supported yet
		public float Pan { get; set; }

		static byte [] tempBuffer1 = new byte [BufferSize];
		static byte [] tempBuffer2 = new byte [BufferSize];

		internal bool StreamBuffer (int buffer)
		{
			int totalRead = 0;
			Utils.BeginTimeMeasurement ();
			while (true) {
				int needToRead = tempBuffer2.Length - totalRead;
				int actuallyRead = decoder.ReadAudioData (tempBuffer1, needToRead);
				if (actuallyRead == 0) {
					if (Looping && needToRead > 0) {
						decoder.Reset ();
					} else {
						break;
					}
				} else if (actuallyRead < 0) {
					throw new Lime.Exception ("Audio decoder returned an error {0}", actuallyRead);
				} else {
					Array.Copy (tempBuffer1, 0, tempBuffer2, totalRead, actuallyRead);
					totalRead += actuallyRead;
				}
			}
			Utils.EndTimeMeasurement ();
			Console.WriteLine ("======================");
			if (totalRead > 0) {
				AL.BufferData (buffer, decoder.Format, tempBuffer2, totalRead, decoder.Frequency);
				AudioSystem.CheckError ();
				return true;
			}
			return false;
		}

		internal void StreamBuffers ()
		{
			int processed = 0;
			if (decoder != null && IsPlaying ()) {
				AL.GetSource (source, ALGetSourcei.BuffersProcessed, out processed);
				AudioSystem.CheckError ();
				while (processed-- > 0) {
					int buffer = AL.SourceUnqueueBuffer (source);
					if (AudioSystem.HasError ()) {
						processed++;
						Thread.Sleep (1);
						continue;
					}
					if (StreamBuffer (buffer)) {
						AL.SourceQueueBuffer (source, buffer);
					}
					if (AudioSystem.HasError ()) {
						processed++;
						Thread.Sleep (1);
						continue;
					}
				}
			}
		}

		internal void ProcessEvents ()
		{
			if (OnStop != null) {
				if (IsStopped ()) {
					var handler = OnStop;
					OnStop = null;
					handler (this);
				}
			}
		}

		public bool IsPaused ()
		{
			return AL.GetSourceState (source) == ALSourceState.Paused;
		}

		public bool IsPlaying ()
		{
			return AL.GetSourceState (source) == ALSourceState.Playing;
		}

		public bool IsInitialState ()
		{
			return AL.GetSourceState (source) == ALSourceState.Initial;
		}

		public bool IsStopped ()
		{
			return AL.GetSourceState (source) == ALSourceState.Stopped;
		}

		public void Stop ()
		{
			AL.SourceStop (source);
			AudioSystem.CheckError ();
			if (OnStop != null)
				OnStop (this);
		}

		public override string ToString ()
		{
			return String.Format ("AudioChannel {0}", id);
		}
	}
}