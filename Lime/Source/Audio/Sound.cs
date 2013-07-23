using System;
#if OPENAL
using OpenTK.Audio.OpenAL;
#endif

namespace Lime
{
	public class Sound
	{
		public Sound()
		{ 
			Channel = NullAudioChannel.Instance;
			IsBumpable = false;
		}
		public bool Loaded { get; internal set; }
		public IAudioChannel Channel { get; internal set; }
		public bool IsBumpable { get; set; }
		
		public float Volume
		{
			get { return Channel.Volume; }
			set { Channel.Volume = value; }
		}

		public float Pitch
		{
			get { return Channel.Pitch; }
			set { Channel.Pitch = value; }
		}

		public float Pan
		{
			get { return Channel.Pan; }
			set { Channel.Pan = value; }
		}

		public void Resume(float fadeinTime = 0) { Channel.Resume(fadeinTime); }
		public bool IsStopped() { return Channel.State == ALSourceState.Stopped; }
		public void Stop(float fadeoutTime = 0) { Channel.Stop(fadeoutTime); }
		public void Bump() { Channel.Bump(); }
	}
}
