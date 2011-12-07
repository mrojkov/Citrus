using System;
using OpenTK.Audio.OpenAL;
using System.Threading;
using System.Runtime.InteropServices;

namespace Lime
{
	public interface IAudioChannel
	{
		ALSourceState State { get; }
		float Pan { get; set; }
		void Resume ();
		void Stop ();
		float Volume { get; set; }
	}

	public class NullAudioChannel : IAudioChannel
	{
		public static NullAudioChannel Instance = new NullAudioChannel ();

		public ALSourceState State { get { return ALSourceState.Stopped; } }
		public float Pan { get { return 0; } set { } }
		public void Resume () {}
		public void Stop () {}
		public float Volume { get { return 0; } set { } }
	}

	public class AudioChannel : IDisposable, IAudioChannel
	{
		public const int BufferSize = 1024 * 16;
		public const int NumBuffers = 8;

		internal AudioChannelGroup Group;
		internal int Priority;
		internal DateTime InitiationTime;
		internal int id;

		object syncRoot = new object ();
		int source;
		float volume = 1;
		int [] buffers;
		int queueHead;
		int queueLength;
		bool looping;

		AudioInstance sound;
		internal IAudioDecoder decoder;
		IntPtr decodedData;
		volatile bool resumePending = false;

		internal AudioChannel (int index)
		{
			this.id = index;
			buffers = AL.GenBuffers (NumBuffers);
			source = AL.GenSource ();
			AudioSystem.CheckError ();
			decodedData = Marshal.AllocHGlobal (BufferSize);
		}

		public void Dispose ()
		{
			if (this.decoder != null) {
				this.decoder.Dispose ();
			}
			Marshal.FreeHGlobal (decodedData);
			AL.SourceStop (source);
			AL.DeleteSource (source);
			AL.DeleteBuffers (buffers);
			AudioSystem.CheckError ();
		}

		public AudioInstance Play (IAudioDecoder decoder, bool looping)
		{
			if (sound != null) {
				sound.Channel = NullAudioChannel.Instance;
			}
			sound = new AudioInstance { Channel = this };
			Stop ();
			this.looping = looping;
			if (this.decoder != null) {
				this.decoder.Dispose ();
			}
			this.decoder = decoder;
			InitiationTime = DateTime.Now;
			int queuedBuffers = 0;
			AL.GetSource (source, ALGetSourcei.BuffersQueued, out queuedBuffers);
			if (queuedBuffers > 0) {
				AL.SourceUnqueueBuffers (source, queuedBuffers);
			}
			AudioSystem.CheckError ();
			queueLength = 0;
			queueHead = 0;
			resumePending = false;
			return sound;
		}

		public void Resume ()
		{
			StreamBuffer ();
			AL.SourcePlay (source);
			AudioSystem.CheckError ();
			//resumePending = true;
		}

		public void Pause ()
		{
			lock (syncRoot) {
				AL.SourcePause (source);
				AudioSystem.CheckError ();
			}
		}

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

		// Not implemented yet
		public float Pan { get; set; }

		internal void ThreadedUpdate ()
		{
			lock (syncRoot) {
				if (State == ALSourceState.Playing) {
					StreamBuffer ();
				}
				if (resumePending) {
					resumePending = false;
					StreamBuffer ();
					AL.SourcePlay (source);
					AudioSystem.CheckError ();
				}
			}
		}

		void StreamBuffer ()
		{
			int buffer = AcquireBuffer ();
			if (buffer != 0) {
				if (LoadBuffer (buffer)) {
					AL.SourceQueueBuffer (source, buffer);
					AudioSystem.CheckError ();
				} else
					queueLength--;
			}
		}

		bool LoadBuffer (int buffer)
		{
			int totalRead = 0;
			int needToRead = BufferSize / decoder.GetBlockSize ();
			while (true) {
				int actuallyRead = decoder.ReadBlocks (decodedData, totalRead, needToRead - totalRead);
				totalRead += actuallyRead;
				if (totalRead == needToRead || !looping) {
					break;
				}
				decoder.Rewind ();
			}
			if (totalRead > 0) {
				AL.BufferData (buffer, decoder.GetFormat (), decodedData, 
					totalRead * decoder.GetBlockSize (), decoder.GetFrequency ());
				return true; 
			}
			return false;
		}

		int AcquireBuffer ()
		{
			int processed = 0;
			AL.GetSource (source, ALGetSourcei.BuffersProcessed, out processed);
			while (processed-- > 0) {
				AL.SourceUnqueueBuffer (source);
				queueLength--;
				queueHead = (queueHead + 1) % NumBuffers;
			}
			AudioSystem.CheckError ();
			if (queueLength == NumBuffers)
				return 0;
			else {
				int index = (queueHead + queueLength++) % NumBuffers;
				return buffers [index];
			}
		}

		public ALSourceState State { get { return AL.GetSourceState (source); } }

		public void Stop ()
		{
			lock (syncRoot) {
				resumePending = false;
				AL.SourceStop (source);
				AudioSystem.CheckError ();
			}
		}
	}
}