#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	internal class AudioChannel : IAudioChannel, IDisposable
	{
		public float Priority;
		public DateTime StartupTime = DateTime.Now;
		private UnityEngine.AudioSource source;
		public int Id;
		private bool paused;
		private float volume = 1;
		private float fadeVolume;
		private float fadeSpeed;
		private volatile int lastBumpedRenderCycle;

		public Sound Sound { get; private set; }

		public AudioChannel(int index)
		{
			var name = "AudioChannel" + index;
			var obj = UnityEngine.GameObject.Find(name);
			if (obj == null) {
				obj = new UnityEngine.GameObject(name);
				obj.AddComponent<UnityEngine.AudioSource>();
			}
			Id = index;
			source = obj.GetComponent<UnityEngine.AudioSource>();
		}

		public void Update(float delta)
		{
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
			} else if (Sound != null && Sound.IsBumpable && Renderer.RenderCycle - lastBumpedRenderCycle > 3) {
				Stop(0.1f);
			}
		}

		internal void Play(Sound sound, UnityEngine.AudioClip clip, bool looping, bool paused, float fadeinTime)
		{
			source.clip = clip;
			if (source.isPlaying) {
				throw new Lime.Exception("AudioSource must be stopped before play");
			}
			source.loop = looping;
			if (Sound != null) {
				Sound.Channel = NullAudioChannel.Instance;
			}
			this.Sound = sound;
			sound.Channel = this;
			StartupTime = DateTime.Now;
			this.paused = true;
			if (!paused) {
				Resume(0);
			}
		}

		public AudioChannelState State { 
			get
			{
				if (paused) {
					return AudioChannelState.Paused;
				} else {
					return source.isPlaying ? AudioChannelState.Playing : AudioChannelState.Stopped; 
				}
			} 
		}

		public AudioChannelGroup Group { get; set; }

		public float Pan { 
			get { return source.pan; }
			set { source.pan = value; }
		}

		public float Volume {
			get { return volume; }
			set
			{
				volume = Mathf.Clamp(value, 0, 1);
				float gain = volume * AudioSystem.GetGroupVolume(Group) * fadeVolume;
				source.volume = gain; 
			}
		}

		public float Pitch { 
			get { return source.pitch; }
			set
			{
				value = Mathf.Clamp(value, 0.0625f, 16);
				source.pitch = value; 
			}
		}

		public string SamplePath { get; set; }

		public void Bump()
		{
			lastBumpedRenderCycle = Renderer.RenderCycle;
		}

		public void Pause()
		{
			paused = true;
			source.Pause();
		}

		public void Resume(float fadeinTime = 0)
		{
			Bump();
			if (fadeinTime > 0) {
				fadeVolume = 0;
				fadeSpeed = 1 / fadeinTime;
			} else {
				fadeSpeed = 0;
				fadeVolume = 1;
			}
			Volume = volume;
			paused = false;
			source.Play();
		}
		
		public void Stop(float fadeoutTime = 0)
		{
			if (fadeoutTime > 0) {
				fadeSpeed = -1 / fadeoutTime;
				return;
			} else {
				fadeSpeed = 0;
				fadeVolume = 0;
			}
			source.Stop();
		}

		public void Dispose() {}
	}
}
#endif