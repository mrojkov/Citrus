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
			get { return source.volume; }
			set { source.volume = value; }
		}

		public float Pitch { 
			get { return source.pitch; }
			set { source.pitch = value; }
		}

		public string SamplePath { get; set; }

		public void Bump() {}

		public void Pause()
		{
			paused = true;
			source.Pause();
		}

		public void Resume(float fadeinTime = 0)
		{
			paused = false;
			source.Play();
		}
		
		public void Stop(float fadeoutTime = 0)
		{
			source.Stop();
		}

		public void Dispose() {}
	}
}
#endif