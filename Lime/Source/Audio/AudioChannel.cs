using System;
#if OPENAL
using OpenTK.Audio.OpenAL;
#endif
using System.Runtime.InteropServices;

#if !OPENAL
public enum ALSourceState
{
	Initial,
	Playing,
	Stopped
}
#endif

namespace Lime
{
	public interface IAudioChannel
	{
		bool Streaming { get; }
		ALSourceState State { get; }
		float Pan { get; set; }
		void Resume(float fadeinTime = 0);
		void Stop(float fadeoutTime = 0);
		float Volume { get; set; }
		float Pitch { get; set; }
		void Bump();
	}

	public class NullAudioChannel : IAudioChannel
	{
		public static NullAudioChannel Instance = new NullAudioChannel();

		public ALSourceState State { get { return ALSourceState.Stopped; } }
		public bool Streaming { get { return false; } }
		public float Pan { get { return 0; } set { } }
		public void Resume(float fadeinTime = 0) {}
		public void Stop(float fadeoutTime = 0) {}
		public float Volume { get { return 0; } set { } }
		public float Pitch { get { return 1; } set { } }
		public void Bump() {}
	}

#if !OPENAL
	internal class AudioChannel : NullAudioChannel, IDisposable
	{
		public AudioChannelGroup Group;
		public float Priority;
		public DateTime StartupTime = DateTime.Now;
		public int Id;

		public AudioChannel(int index)
		{
			Id = index;
		}

		public void Update(float delta)
		{
		}

		public Sound Play(IAudioDecoder decoder, bool looping)
		{
			return new Sound();
		}

		public void Pause()
		{
		}

		public void Dispose()
		{
		}
	}

#else
	internal class AudioChannel : IDisposable, IAudioChannel
	{
		public const int BufferSize = 1024 * 32;
		public const int NumBuffers = 8;

		public AudioChannelGroup Group;
		public float Priority;
		public DateTime StartupTime;
		public int Id;

		private object streamingSync = new object();
		private volatile bool streaming;

		private int source;
		private float volume = 1;
		private float pitch = 1;
		private int[] buffers;
		private int queueHead;
		private int queueLength;
		private bool looping;
		private float fadeVolume;
		private float fadeSpeed;
		private int lastBumpedRenderCycle = 0;

		private Sound sound = null;
		private IAudioDecoder decoder;
		private IntPtr decodedData;

		// The channel can be locked while a sound is being preloaded
		public bool Locked { get; set; }

		public bool Streaming { get { return streaming; } }
		
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
			if (decoder != null) {
				decoder.Dispose();
			}
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

		internal void Play(Sound sound, IAudioDecoder decoder, bool looping, bool paused, float fadeinTime)
		{
			lock (streamingSync) {
				if (streaming) {
					throw new Lime.Exception("Can't play on the channel because it is already being played");
				}
				this.looping = looping;
				if (this.decoder != null) {
					this.decoder.Dispose();
				}
				this.decoder = decoder;
			}
			if (this.sound != null) {
				this.sound.Channel = NullAudioChannel.Instance;
			}
			sound.Channel = this;
			StartupTime = DateTime.Now;
			if (!paused) {
				Resume(fadeinTime);
			}
		}

		public void Resume(float fadeinTime = 0)
		{
			if (decoder == null) {
				throw new InvalidOperationException("Can't resume sound before it has decoded");
			}
			if (fadeinTime > 0) {
				fadeVolume = 0;
				fadeSpeed = 1 / fadeinTime;
			} else {
				fadeSpeed = 0;
				fadeVolume = 1;
			}
			Volume = volume;
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

		public void Stop(float fadeoutTime = 0)
		{
			if (fadeoutTime > 0) {
				// fadeVolume = 1;
				fadeSpeed = -1 / fadeoutTime;
				return;
			} else {
				fadeSpeed = 0;
				fadeVolume = 0;
			}
			lock (streamingSync) {
				streaming = false;
				AL.SourceStop(source);
				AudioSystem.SuppressError("AudioChannel.Stop");
			}
		}

		public float Pitch
		{
			get { return pitch; }
			set 
			{
				pitch = Mathf.Clamp(value, 0.0625f, 16);
				AL.Source(source, ALSourcef.Pitch, pitch);
				AudioSystem.CheckError();
			}
		}

		public float Volume
		{
			get { return volume; }
			set
			{
				volume = Mathf.Clamp(value, 0, 1);
				float gain = volume * AudioSystem.GetGroupVolume(Group) * fadeVolume;
				AL.Source(source, ALSourcef.Gain, gain);
				AudioSystem.CheckError();
			}
		}

		public float Pan { get; set; }

		public void Bump()
		{
			lastBumpedRenderCycle = Renderer.RenderCycle;
		}

		public void Update(float delta)
		{
			if (streaming) {
				lock (streamingSync) {
					if (streaming) {
						UpdateHelper();
					}
				}
			}
			if (fadeSpeed != 0) {
				fadeVolume += delta * fadeSpeed;
				if (fadeVolume > 1) {
					fadeSpeed = 0;
					fadeVolume = 1;
				} else if (fadeVolume < 0) {
					fadeSpeed = 0;
					fadeVolume = 0;
					Stop();
				}
				Volume = volume;
			}
			if (sound != null && sound.IsBumpable && Renderer.RenderCycle - lastBumpedRenderCycle > 3)
				Stop(0.1f);
		}

		void UpdateHelper()
		{
			if (decoder == null) {
				throw new InvalidOperationException("AudioChannel is streaming while decoder is not set");
			}
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
			switch (State) {
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
				AudioSystem.SuppressError("AudioChannel.FillupBuffer");
				ALFormat format = (decoder.GetFormat() == AudioFormat.Stereo16) ? ALFormat.Stereo16 : ALFormat.Mono16;
				AL.BufferData(buffer, format, decodedData, 
					totalRead * decoder.GetBlockSize(), decoder.GetFrequency());
				AL.GetError(); // Suppress any possible error
				// AudioSystem.CheckError();
				return true;
			}
			return false;
		}

		void UnqueueBuffers()
		{
			int processed;
			AL.GetSource(source, ALGetSourcei.BuffersProcessed, out processed);
#if iOS
			// This is workaround for a bug in iOS OpenAl implementation.
			// When AL.SourceStop() has called, the number of processed buffers exceedes total number of buffers.
			processed = Math.Min(queueLength, processed);
#endif
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
#endif
}