using System;
using OpenTK.Audio.OpenAL;
using System.Threading;
using ProtoBuf;
using System.Runtime.InteropServices;

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
		public const int BufferSize = 1024 * 16;
		public const int NumBuffers = 8;
		public delegate void StopEvent (AudioChannel channel);
		public StopEvent OnStop;
		public bool Looping;
		
		internal AudioChannelGroup Group;
		internal int Priority;
		internal DateTime InitiationTime;
		internal int id;

		object syncRoot = new object ();
		int source;
		int [] buffers;
		int queueHead;
		int queueLength;

		internal IAudioDecoder decoder;
		IntPtr rawSound;
		volatile bool resumePending = false;

		internal AudioChannel (int index)
		{
			this.id = index;
			buffers = AL.GenBuffers (NumBuffers);
			source = AL.GenSource ();
			AudioSystem.CheckError ();
			rawSound = Marshal.AllocHGlobal (BufferSize);
		}

		public void Dispose ()
		{
			if (this.decoder != null) {
				this.decoder.Dispose ();
			}
			Marshal.FreeHGlobal (rawSound);
			AL.SourceStop (source);
			AL.DeleteSource (source);
			AL.DeleteBuffers (buffers);
			AudioSystem.CheckError ();
		}

		internal void PlaySound (IAudioDecoder decoder, bool looping)
		{
			lock (syncRoot) {
				this.Looping = looping;
				if (this.decoder != null) {
					this.decoder.Dispose ();
				}
				this.decoder = decoder;
				InitiationTime = DateTime.Now;
				AL.SourceStop (source);
				int queuedBuffers = 0;
				AL.GetSource (source, ALGetSourcei.BuffersQueued, out queuedBuffers);
				if (queuedBuffers > 0) {
					AL.SourceUnqueueBuffers (source, queuedBuffers);
				}
				AudioSystem.CheckError ();
				queueLength = 0;
				queueHead = 0;
				resumePending = false;
			}
		}

		public void Resume ()
		{
			resumePending = true;
		}

		/// <summary>
		/// Pauses channel. Used only for pausing on application deactivation.
		/// </summary>
		internal void Pause ()
		{
			lock (this) {
				AL.SourcePause (source);
				AudioSystem.CheckError ();
			}
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

		// Not implemented yet
		public float Pan { get; set; }

		internal void ThreadedUpdate ()
		{
			lock (syncRoot) {
				if (IsPlaying ()) {
					StreamBuffer ();
				}
				if (resumePending) {
					resumePending = false;
					StreamBuffer ();
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
				int actuallyRead = decoder.ReadBlocks (rawSound, totalRead, needToRead - totalRead);
				totalRead += actuallyRead;
				if (totalRead == needToRead || !Looping) {
					break;
				}
				decoder.ResetToBeginning ();
			}
			if (totalRead > 0) {
				AL.BufferData (buffer, decoder.GetFormat (), rawSound, 
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

		internal void ProcessEvents ()
		{
			if (OnStop != null) {
				if (IsStopped ()) {
					var handler = OnStop;
					OnStop = null;
					//if (decoder != null) {
					//	decoder.Dispose ();
					//	decoder = null;
					//}
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
			lock (this) {
				AL.SourceStop (source);
				AudioSystem.CheckError ();
				if (OnStop != null)
					OnStop (this);
			}
		}

		public override string ToString ()
		{
			return String.Format ("AudioChannel {0}", id);
		}
	}
}