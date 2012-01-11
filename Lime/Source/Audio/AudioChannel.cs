using System;
using OpenTK.Audio.OpenAL;
using System.Runtime.InteropServices;

namespace Lime
{
	public interface IAudioChannel
	{
		ALSourceState State { get; }
		float Pan { get; set; }
		void Resume();
		void Stop();
		float Volume { get; set; }
	}

	public class NullAudioChannel : IAudioChannel
	{
		public static NullAudioChannel Instance = new NullAudioChannel();

		public ALSourceState State { get { return ALSourceState.Stopped; } }
		public float Pan { get { return 0; } set { } }
		public void Resume() {}
		public void Stop() {}
		public float Volume { get { return 0; } set { } }
	}

	internal class AudioChannel : IDisposable, IAudioChannel
	{
		public const int BufferSize = 1024 * 16;
		public const int NumBuffers = 8;

		public AudioChannelGroup Group;
		public int Priority;
		public DateTime StartupTime;
		public int Id;

		object streamingSync = new object();
		volatile bool streaming;

		int source;
		float volume = 1;
		int[] buffers;
		int queueHead;
		int queueLength;
		bool looping;

		Sound sound;
		internal IAudioDecoder decoder;
		IntPtr decodedData;

		public AudioChannel(int index)
		{
			this.Id = index;
			buffers = AL.GenBuffers(NumBuffers);
			source = AL.GenSource();
			AudioSystem.CheckError();
			decodedData = Marshal.AllocHGlobal(BufferSize);
		}

		public void Dispose()
		{
			if (decoder != null)
				decoder.Dispose();
			Marshal.FreeHGlobal(decodedData);
			AL.SourceStop(source);
			AL.DeleteSource(source);
			AL.DeleteBuffers(buffers);
			AudioSystem.CheckError();
		}

		public ALSourceState State { 
			get {
				return AL.GetSourceState(source); 
			}
		}

		public bool IsFree()
		{
			if (streaming) {
				return false;
			} else {
				UnqueueBuffers();
				int queued;
				AL.GetSource(source, ALGetSourcei.BuffersQueued, out queued);
				AudioSystem.CheckError();
				return queued == 0;
			}
		}
		
		public Sound Play(IAudioDecoder decoder, bool looping)
		{
			if (streaming) {
				throw new Lime.Exception("Can't play on the channel");
			}
			lock (streamingSync) {
				this.looping = looping;
				if (this.decoder != null)
					this.decoder.Dispose();
				this.decoder = decoder;
			}
			if (sound != null) {
				sound.Channel = NullAudioChannel.Instance;
			}
			sound = new Sound { Channel = this };
			StartupTime = DateTime.Now;
			return sound;
		}

		public void Resume()
		{
			streaming = true;
			if (State == ALSourceState.Paused) {
				AL.SourcePlay(source);
			}
		}

		public void Pause()
		{
			AL.SourcePause(source);
			AudioSystem.CheckError();
		}

		public void Stop()
		{
			streaming = false;
			AL.SourceStop(source);
			AudioSystem.CheckError();
		}

		public float Volume
		{
			get { return volume; }
			set
			{
				volume = Utils.Clamp(value, 0, 1);
				float gain = volume * AudioSystem.GetGroupVolume(Group);
				AL.Source(source, ALSourcef.Gain, gain);
				AudioSystem.CheckError();
			}
		}

		public float Pan { get; set; }

		public void Update()
		{
			if (streaming) {
				lock (streamingSync) {
					if (streaming) {
						UpdateHelper();
					}
				}
			}
		}

		void UpdateHelper()
		{
			// queue one buffer
			int buffer = AcquireBuffer();
			if (buffer != 0) {
				if (FillupBuffer(buffer)) {
					AL.SourceQueueBuffer(source, buffer);
					AudioSystem.CheckError();
				} else {
					queueLength--;
					streaming = false;
				}
			}
			// resume playing
			switch(State) {
			case ALSourceState.Stopped:
			case ALSourceState.Initial:
				AL.SourcePlay(source);
				break;
			}
		}

		bool FillupBuffer(int buffer)
		{
			int totalRead = 0;
			int needToRead = BufferSize / decoder.GetBlockSize();
			while (true) {
				int actuallyRead = decoder.ReadBlocks(decodedData, totalRead, needToRead - totalRead);
				totalRead += actuallyRead;
				if (totalRead == needToRead || !looping) {
					break;
				}
				decoder.Rewind();
			}
			if (totalRead > 0) {
				AudioSystem.CheckError();
				AL.BufferData(buffer, decoder.GetFormat(), decodedData, 
					totalRead * decoder.GetBlockSize(), decoder.GetFrequency());
				AudioSystem.CheckError();
				return true; 
			}
			return false;
		}

		void UnqueueBuffers()
		{
			int processed;
			AL.GetSource(source, ALGetSourcei.BuffersProcessed, out processed);
			while (processed-- > 0) {
				AL.SourceUnqueueBuffer(source);
				queueLength--;
				queueHead = (queueHead + 1) % NumBuffers;
			}
		}

		int AcquireBuffer()
		{
			UnqueueBuffers();
			if (queueLength == NumBuffers) {
				return 0;
			} else {
				int index = (queueHead + queueLength++) % NumBuffers;
				return buffers[index];
			}
		}
	}
}