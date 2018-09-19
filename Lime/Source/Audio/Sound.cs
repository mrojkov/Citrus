#if OPENAL
#if !MONOMAC
using OpenTK.Audio.OpenAL;
#else
using MonoMac.OpenAL;
#endif
#endif

namespace Lime
{
	public class ChannelParameters
	{
		public IAudioDecoder Decoder;
		public AudioChannelGroup Group;
		public float Pan;
		public float Volume;
		public float Pitch;
		public string SamplePath;
		public float Priority;
		public bool Looping;
	}

	public class Sound
	{
		public Sound()
		{
			Channel = NullAudioChannel.Instance;
		}

		public IAudioChannel Channel { get; internal set; }

		public System.Func<bool> StopChecker;

		public bool IsLoading { get; internal set; }

		public bool IsStopped { get { return Channel.State == AudioChannelState.Stopped; } }

		private ChannelParameters suspendedChannelParameters;

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

		public void Resume(float fadeinTime = 0)
		{
			EnsureLoaded();
			if (suspendedChannelParameters != null) {
				AudioSystem.ResumeSound(this, suspendedChannelParameters, fadeinTime);
				suspendedChannelParameters = null;
			} else {
				Channel.Resume(fadeinTime);
			}
		}

		public void Stop(float fadeoutTime = 0)
		{
			EnsureLoaded();
			Channel.Stop(fadeoutTime);
		}

		public void Pause(float fadeoutTime = 0)
		{
			EnsureLoaded();
			Channel.Pause(fadeoutTime);
		}

		public void Suspend(float fadeoutTime = 0)
		{
			EnsureLoaded();
			suspendedChannelParameters = Channel.Suspend(fadeoutTime);
		}

		private void EnsureLoaded()
		{
			if (IsLoading) {
				throw new System.InvalidOperationException("The sound is being loaded");
			}
		}
	}
}
