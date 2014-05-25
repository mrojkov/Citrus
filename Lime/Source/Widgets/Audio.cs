using System;
using Lime;
using ProtoBuf;
using System.ComponentModel;

namespace Lime
{
	[ProtoContract]
	public enum AudioAction
	{
		[ProtoEnum]
		Play,
		[ProtoEnum]
		Stop
	}

	[ProtoContract]
	public class Audio : Node
	{
		Sound sound = new Sound() { IsBumpable = true };

		[ProtoMember(1)]
		public SerializableSample Sample { get; set; }

		[ProtoMember(2)]
		public bool Looping { get; set; }

		[ProtoMember(3)]
		public float FadeTime { get; set; }

		private float volume = 0.5f;
		[ProtoMember(4)]
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
		[ProtoMember(5)]
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
		[ProtoMember(9)]
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
		public AudioAction Action { get; set; }

		[ProtoMember(7)]
		public AudioChannelGroup Group { get; set; }

		[ProtoMember(10)]
		public float Priority { get; set; }

		public Audio()
		{
			Priority = 0.5f;
		}

		public void Play()
		{
			sound = Sample.Play(Group, false, 0f, Looping, Priority, Volume, Pan, Pitch);
			sound.IsBumpable = true;
		}

		public void Stop()
		{
			sound.Stop(FadeTime);
		}

		public bool IsPlaying()
		{
			return !sound.IsStopped;
		}

		protected override void SelfUpdate(float delta)
		{
			sound.Bump();
		}

		protected internal override void OnTrigger(string property)
		{
			if (property == "Action") {
				if (Action == AudioAction.Play) {
					Play();
				} else {
					Stop();
				}
			} else {
				base.OnTrigger(property);
			}
		}
	}
}
