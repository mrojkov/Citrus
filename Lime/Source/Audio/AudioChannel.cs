using System;
using OpenTK.Audio.OpenAL;
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

	internal class AudioChannel : IDisposable, IAudioChannel
	{
		public const int BufferSize = 1024 * 32;
		public const int NumBuffers = 4;

		public AudioChannelGroup Group;
		public int Priority;
		public DateTime StartupTime;
		public int Id;

		object streamingSync = new object ();

		int source;
		float volume = 1;
		int [] buffers;
		int queueHead;
		int queueLength;
		bool looping;

		Sound sound;
		internal IAudioDecoder decoder;
		IntPtr decodedData;

		public AudioChannel (int index)
		{
			this.Id = index;
			buffers = AL.GenBuffers (NumBuffers);
			source = AL.GenSource ();
			AudioSystem.CheckError ();
			decodedData = Marshal.AllocHGlobal (BufferSize);
		}

		public void Dispose ()
		{
			if (decoder != null)
				decoder.Dispose ();
			Marshal.FreeHGlobal (decodedData);
			AL.SourceStop (source);
			AL.DeleteSource (source);
			AL.DeleteBuffers (buffers);
			AudioSystem.CheckError ();
		}

		public ALSourceState State { get { return AL.GetSourceState (source); } }

		public Sound Play (IAudioDecoder decoder, bool looping)
		{
			Stop ();
			if (sound != null)
				sound.Channel = NullAudioChannel.Instance;
			sound = new Sound { Channel = this };
			this.looping = looping;
			if (this.decoder != null)
				this.decoder.Dispose ();
			this.decoder = decoder;
			int queuedBuffers = 0;
			AL.GetSource (source, ALGetSourcei.BuffersQueued, out queuedBuffers);
			if (queuedBuffers > 0) {
				AL.SourceUnqueueBuffers (source, queuedBuffers);
			}
			AudioSystem.CheckError ();
			queueLength = 0;
			queueHead = 0;
			StreamBuffer ();
			StartupTime = DateTime.Now;
			return sound;
		}

		public void Resume ()
		{
			AL.SourcePlay (source);
			AudioSystem.CheckError ();
		}

		public void Pause ()
		{
			AL.SourcePause (source);
			AudioSystem.CheckError ();
		}

		public void Stop ()
		{
			lock (streamingSync) {
				AL.SourceStop (source);
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

		public float Pan { get; set; }

		public void Update ()
		{
			if (State == ALSourceState.Playing) {
				lock (streamingSync) {
					if (State == ALSourceState.Playing)
						StreamBuffer ();
				}
			}
		}

		void StreamBuffer ()
		{
			int buffer = AcquireBuffer ();
			if (buffer != 0) {
				if (FillupBuffer (buffer)) {
					AL.SourceQueueBuffer (source, buffer);
					AudioSystem.CheckError ();
				} else
					queueLength--;
			}
		}

		bool FillupBuffer (int buffer)
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
	}
}