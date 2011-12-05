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
		public const int BufferSize = 1024 * 32;
		public const int NumBuffers = 3;

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
		IntPtr tempBuffer;

		internal AudioChannel (int index)
		{
			this.id = index;
			buffers = AL.GenBuffers (NumBuffers);
			source = AL.GenSource ();
			AudioSystem.CheckError ();
			tempBuffer = Marshal.AllocHGlobal (BufferSize);
		}

		public void Dispose ()
		{
			if (this.decoder != null) {
				this.decoder.Dispose ();
			}
			Marshal.FreeHGlobal (tempBuffer);
			AL.SourceStop (source);
			AL.DeleteSource (source);
			AL.DeleteBuffers (buffers);
			AudioSystem.CheckError ();
		}

		internal void PlaySound (IAudioDecoder decoder, bool looping)
		{
			this.Looping = looping;
			if (this.decoder != null) {
				this.decoder.Dispose ();
			}
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

		bool StreamBuffer (int buffer)
		{
			int totalRead = 0;
			int needToRead = BufferSize / decoder.GetBlockSize ();
			while (true) {
				int actuallyRead = decoder.ReadBlocks (tempBuffer, totalRead, needToRead - totalRead);
				totalRead += actuallyRead;
				if (totalRead == needToRead || !Looping) {
					break;
				}
				decoder.ResetToBeginning ();
			}
			if (totalRead > 0) {
				AL.BufferData (buffer, decoder.GetFormat (), tempBuffer, totalRead * decoder.GetBlockSize (), decoder.GetFrequency ());
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
					if (decoder != null) {
						decoder.Dispose ();
						decoder = null;
					}
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