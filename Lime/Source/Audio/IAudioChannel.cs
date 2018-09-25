
namespace Lime
{
	public enum AudioChannelState
	{
		Initial,
		Playing,
		Stopped,
		Paused
	}

	public enum AudioChannelGroup
	{
		Effects,
		Music,
		Voice
	}

	public interface IAudioChannel
	{
		AudioChannelState State { get; }
		AudioChannelGroup Group { get; }
		float Pan { get; }
		float Volume { get; }
		float Pitch { get; }
		string SamplePath { get; }
		Sound Sound { get; }
	}

	internal interface IAudioChannelInternal
	{
		AudioChannelState State { get; }
		AudioChannelGroup Group { get; set; }
		float Pan { get; set; }
		float Volume { get; set; }
		float Pitch { get; set; }
		string SamplePath { get; set; }

		void Resume(float fadeinTime = 0);
		void Stop(float fadeoutTime = 0);
		void Pause(float fadeoutTime = 0);
		PlayParameters Suspend(float fadeoutTime = 0);
	}

	public class NullAudioChannel : IAudioChannelInternal, IAudioChannel
	{
		public static NullAudioChannel Instance = new NullAudioChannel();

		public AudioChannelState State { get { return AudioChannelState.Stopped; } }
		public AudioChannelGroup Group { get; set; }
		public float Pan { get { return 0; } set { } }
		public void Resume(float fadeinTime = 0) {}
		public void Stop(float fadeoutTime = 0) {}
		public void Pause(float fadeoutTime = 0) {}
		public PlayParameters Suspend(float fadeoutTime = 0) { return null; }
		public float Volume { get { return 0; } set { } }
		public float Pitch { get { return 1; } set { } }
		public string SamplePath { get; set; }
		public Sound Sound { get { return null; } }
	}
}
