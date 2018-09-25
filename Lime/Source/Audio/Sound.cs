#if OPENAL
#if !MONOMAC
using OpenTK.Audio.OpenAL;
#else
using MonoMac.OpenAL;
#endif
#endif

namespace Lime
{
	public class Sound
	{
		public Sound()
		{
			ChannelInternal = NullAudioChannel.Instance;
		}

		public IAudioChannel Channel => (IAudioChannel)ChannelInternal;

		internal IAudioChannelInternal ChannelInternal { get; set; }

		public System.Func<bool> StopChecker;

		public bool IsLoading { get; internal set; }

		public bool IsStopped { get { return ChannelInternal.State == AudioChannelState.Stopped; } }

		public float Volume
		{
			get { return ChannelInternal.Volume; }
			set { ChannelInternal.Volume = value; }
		}

		public float Pitch
		{
			get { return ChannelInternal.Pitch; }
			set { ChannelInternal.Pitch = value; }
		}

		public float Pan
		{
			get { return ChannelInternal.Pan; }
			set { ChannelInternal.Pan = value; }
		}

		public void Resume(float fadeinTime = 0)
		{
			EnsureLoaded();
			ChannelInternal.Resume(fadeinTime);
		}

		public void Stop(float fadeoutTime = 0)
		{
			EnsureLoaded();
			ChannelInternal.Stop(fadeoutTime);
		}

		public void Pause(float fadeoutTime = 0)
		{
			EnsureLoaded();
			ChannelInternal.Pause(fadeoutTime);
		}

		public PlayParameters Suspend(float fadeoutTime = 0)
		{
			EnsureLoaded();
			return ChannelInternal.Suspend(fadeoutTime);
		}

		private void EnsureLoaded()
		{
			if (IsLoading) {
				throw new System.InvalidOperationException("The sound is being loaded");
			}
		}
	}
}
