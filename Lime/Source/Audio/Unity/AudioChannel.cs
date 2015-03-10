#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	internal class AudioChannel : NullAudioChannel, IDisposable
	{
		public AudioChannelGroup Group;
		public float Priority;
		public DateTime StartupTime = DateTime.Now;
		public AudioFormat AudioFormat { get; private set; }
		public int Id;

		public AudioChannel(int index, AudioFormat format)
		{
			Id = index;
			AudioFormat = format;
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
}
#endif