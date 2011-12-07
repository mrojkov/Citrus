using System;
using OpenTK.Audio.OpenAL;

namespace Lime
{
	public class AudioInstance
	{
		public AudioInstance () { Channel = NullAudioChannel.Instance; }

		public IAudioChannel Channel { get; internal set; }
		
		public float Volume
		{
			get { return Channel.Volume; }
			set { Channel.Volume = value; }
		}

		public float Pan
		{
			get { return Channel.Pan; }
			set { Channel.Pan = value; }
		}

		public void Resume () { Channel.Resume (); }
		public bool IsStopped () { return Channel.State == ALSourceState.Stopped; }
		public void Stop () { Channel.Stop (); }
	}
}
