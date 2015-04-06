#if OPENAL
using System;
using System.Collections.Generic;
using OpenTK.Audio.OpenAL;
using System.Runtime.InteropServices;

namespace Lime
{
	internal class AudioChannel : IDisposable, IAudioChannel
	{
		public const int BufferSize = 1024 * 32;
		public const int NumBuffers = 8;

		public AudioChannelGroup Group { get; set; }
		public float Priority;
		public DateTime StartupTime;
		public int Id;

		private readonly object streamingSync = new object();
		private volatile bool streaming;

		private readonly int source;
		private float volume = 1;
		private float pitch = 1;
		private float pan = 0;
		private bool looping;
		private float fadeVolume;
		private float fadeSpeed;
		private volatile int lastBumpedRenderCycle;
		private List<int> allBuffers;
		private Stack<int> processedBuffers;

		private IAudioDecoder decoder;
		private readonly IntPtr decodedData;
		
		public bool Streaming { get { return streaming; } }

		public float Pitch
		{
			get { return pitch; }
			set { SetPitch(value); }
		}

		public float Volume
		{
			get { return volume; }
			set { SetVolume(value); }
		}

		public AudioChannelState State
		{
			get
			{
				switch (AL.GetSourceState(source)) {
					case ALSourceState.Initial:
						return AudioChannelState.Initial;
					case ALSourceState.Paused:
						return AudioChannelState.Paused;
					case ALSourceState.Playing:
						return AudioChannelState.Playing;
					case ALSourceState.Stopped:
					default:
						return AudioChannelState.Stopped;
				}
			}
		}

		public Sound Sound { get; private set; }

		public float Pan {
			get { return pan; }
			set { SetPan(value); }
		}

		public string SamplePath { get; set; }

		public AudioFormat AudioFormat { get; private set; }

		public AudioChannel(int index, AudioFormat format)
		{
			AudioFormat = format;
			Sound = null;
			this.Id = index;
			decodedData = Marshal.AllocHGlobal(BufferSize);
			using (new PlatformAudioSystem.ErrorChecker()) {
				allBuffers = new List<int>(AL.GenBuffers(NumBuffers));
				source = AL.GenSource();
			}
			processedBuffers = new Stack<int>(allBuffers);
		}
		
		public void Dispose()
		{
			if (!AudioSystem.Active) {
				return;
			}
			if (decoder != null) {
				decoder.Dispose();
			}
			AL.SourceStop(source);
			AL.DeleteSource(source);
			AL.DeleteBuffers(allBuffers.ToArray());
			Marshal.FreeHGlobal(decodedData);
		}

		private void SetPan(float value)
		{
			pan = value.Clamp(-1, 1);
			var sourcePosition = Vector2.HeadingRad(pan * Mathf.HalfPi);
			using (new PlatformAudioSystem.ErrorChecker()) {
				AL.Source(source, ALSource3f.Position, sourcePosition.Y, 0, sourcePosition.X);
			}
		}

		internal void Play(Sound sound, IAudioDecoder decoder, bool looping, bool paused, float fadeinTime)
		{
			if (!AudioSystem.Active) {
				return;
			}
			var state = AL.GetSourceState(source);
			if (state != ALSourceState.Initial && state != ALSourceState.Stopped) {
				throw new Lime.Exception("AudioSource must be stopped before play");
			}
			lock (streamingSync) {
				if (streaming) {
					throw new Lime.Exception("Can't play on channel because it is in use");
				}
				this.looping = looping;
				if (this.decoder != null) {
					this.decoder.Dispose();
				}
				this.decoder = decoder;
			}
			DetachBuffers();
			if (Sound != null) {
				Sound.Channel = NullAudioChannel.Instance;
			}
			this.Sound = sound;
			sound.Channel = this;
			StartupTime = DateTime.Now;
			if (!paused) {
				Resume(fadeinTime);
			}
		}

		private void DetachBuffers()
		{
			using (new PlatformAudioSystem.ErrorChecker(throwException: false)) {
				AL.Source(source, ALSourcei.Buffer, 0);
			}
			processedBuffers = new Stack<int>(allBuffers);
		}

		public void Resume(float fadeinTime = 0)
		{
			if (!AudioSystem.Active) {
				return;
			}
			Bump();
			if (decoder == null) {
				throw new InvalidOperationException("Audio decoder is not set");
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
			if (State == AudioChannelState.Paused) {
				using (new PlatformAudioSystem.ErrorChecker()) {
					AL.SourcePlay(source);
				}
			}
		}

		public void Pause()
		{
			if (!AudioSystem.Active) {
				return;
			}
			using (new PlatformAudioSystem.ErrorChecker()) {
				AL.SourcePause(source);
			}
		}

		public void Stop(float fadeoutTime = 0)
		{
			if (!AudioSystem.Active) {
				return;
			}
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
				using (new PlatformAudioSystem.ErrorChecker(throwException: false)) {
					AL.SourceStop(source);
				}
			}
		}

		private void SetPitch(float value)
		{
			if (!AudioSystem.Active) {
				return;
			}
			pitch = Mathf.Clamp(value, 0.0625f, 16);
			using (new PlatformAudioSystem.ErrorChecker()) {
				AL.Source(source, ALSourcef.Pitch, pitch);
			}
		}

		private void SetVolume(float value)
		{
			if (!AudioSystem.Active) {
				return;
			}
			volume = Mathf.Clamp(value, 0, 1);
			float gain = volume * AudioSystem.GetGroupVolume(Group) * fadeVolume;
			using (new PlatformAudioSystem.ErrorChecker()) {
				AL.Source(source, ALSourcef.Gain, gain);
			}
		}

		public void Bump()
		{
			lastBumpedRenderCycle = Renderer.RenderCycle;
		}

		public void Update(float delta)
		{
			if (!AudioSystem.Active) {
				return;
			}
			if (streaming) {
				lock (streamingSync) {
					if (streaming) {
						QueueBuffers();
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
			} else if (streaming && Sound != null && Sound.IsBumpable && Renderer.RenderCycle - lastBumpedRenderCycle > 3) {
				Stop(0.1f);
			}
		}

		void QueueBuffers()
		{
			if (decoder == null) {
				throw new InvalidOperationException("Audio decoder is not set");
			}
			UnqueueProcessedBuffers();
			bool addedbuffers = false;
			while (QueueOneBuffer()) {
				addedbuffers = true;
			}
			if (addedbuffers) {
				if (State == AudioChannelState.Stopped || State == AudioChannelState.Initial) {
					AL.SourcePlay(source);
				}
			}
		}

		bool QueueOneBuffer()
		{
			int buffer = AcquireBuffer();
			if (buffer != 0) {
				if (FillBuffer(buffer)) {
					// iOS OpenAL implementation is buggy, so protect ourselves
					if (AL.GetError() != ALError.NoError) {
						RecreateBuffers();
					} else {
						AL.SourceQueueBuffer(source, buffer);
						if (AL.GetError() != ALError.NoError) {
							RecreateBuffers();
						}
					}
					return true;
				} else {
					processedBuffers.Push(buffer);
					streaming = false;
				}
			}
			return false;
		}

		private void RecreateBuffers()
		{
			Logger.Write("Recreating audio buffers (sample: {0})", SamplePath);
			DetachBuffers();
			foreach (var buffer in allBuffers) {
				AL.DeleteBuffer(buffer);
				AL.GetError();
			}
			allBuffers = new List<int>(AL.GenBuffers(NumBuffers));
			processedBuffers = new Stack<int>(allBuffers);
		}

		private bool FillBuffer(int buffer)
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
				ALFormat format = (decoder.GetFormat() == AudioFormat.Stereo16) ? ALFormat.Stereo16 : ALFormat.Mono16;
				int dataSize = totalRead * decoder.GetBlockSize();
				AL.BufferData(buffer, format, decodedData, dataSize, decoder.GetFrequency());
				return true;
			}
			return false;
		}

		void UnqueueProcessedBuffers()
		{
			AL.GetError();
			int numProcessed;
			AL.GetSource(source, ALGetSourcei.BuffersProcessed, out numProcessed);
			for (int i = 0; i < numProcessed; i++) {
				int buffer = AL.SourceUnqueueBuffer(source);
				if (buffer != 0) {
					processedBuffers.Push(buffer);
				}
			}
		}
		
		int AcquireBuffer()
		{
			int c = processedBuffers.Count;
			if (c == 0) {
				return 0;
			} else {
				var buffer = processedBuffers.Pop();
				return buffer;
			}
		}
	}
}
#endif