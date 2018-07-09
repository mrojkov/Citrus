using Yuzu;

namespace Lime
{
	public enum AudioAction
	{
		Play,
		Stop
	}

	public class Audio : Node
	{
		public static bool GloballyEnable = true;

		Sound sound = new Sound() { IsBumpable = true };

		[YuzuMember]
		[TangerineKeyframeColor(19)]
		public SerializableSample Sample { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(20)]
		public bool Looping { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(21)]
		public float FadeTime { get; set; }

		private float volume = 0.5f;

		[YuzuMember]
		[TangerineKeyframeColor(22)]
		public float Volume
		{
			get { return volume; }
			set
			{
				volume = value;
				sound.Volume = volume;
			}
		}

		private float pan = 0;

		[YuzuMember]
		[TangerineKeyframeColor(23)]
		public float Pan
		{
			get { return pan; }
			set
			{
				pan = value;
				sound.Pan = pan;
			}
		}

		private float pitch = 1;

		[YuzuMember]
		[TangerineKeyframeColor(24)]
		public float Pitch
		{
			get { return pitch; }
			set
			{
				pitch = value;
				sound.Pitch = pitch;
			}
		}

		[Trigger]
		[TangerineKeyframeColor(15)]
		public AudioAction Action { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(25)]
		public AudioChannelGroup Group { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(26)]
		public float Priority { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(27)]
		public bool Bumpable { get; set; }

		public Audio()
		{
			RenderChainBuilder = null;
			Priority = 0.5f;
			Bumpable = true;
		}

		public void Play()
		{
			sound = Sample.Play(Group, false, 0f, Looping, Priority, Volume, Pan, Pitch);
			sound.IsBumpable = Bumpable;
		}

		public void Stop()
		{
			sound.Stop(FadeTime);
		}

		public bool IsPlaying()
		{
			return !sound.IsStopped;
		}

		public override void Update(float delta)
		{
			base.Update(delta);
			sound.Bump();
		}

		public override void AddToRenderChain(RenderChain chain)
		{
		}

		public override void OnTrigger(string property, double animationTimeCorrection = 0)
		{
			if (property == "Action") {
				if (GloballyEnable) {
					if (Action == AudioAction.Play) {
						Play();
					} else {
						Stop();
					}
				}
			} else {
				base.OnTrigger(property);
			}
		}
	}
}
