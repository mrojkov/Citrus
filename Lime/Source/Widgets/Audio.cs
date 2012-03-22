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
		Sound sound = new Sound();

		[ProtoMember(1)]
		public SerializableSample Sample { get; set; }

		[ProtoMember(2), DefaultValue(false)]
		public bool Looping { get; set; }

		[ProtoMember(3), DefaultValue(0)]
		public float FadeTime { get; set; }

		float volume = 1;
		[ProtoMember(4), DefaultValue(1)]
		public float Volume
		{
			get { return volume; }
			set
			{
				volume = value;
				sound.Volume = volume;
			}
		}

		float pan = 0;
		[ProtoMember(5), DefaultValue(0)]
		public float Pan
		{
			get { return pan; }
			set
			{
				pan = value;
				sound.Pan = pan;
			}
		}

		[Trigger]
		public AudioAction Action { get; set; }

		[ProtoMember(7), DefaultValue(AudioChannelGroup.Effects)]
		public AudioChannelGroup Group { get; set; }

		[ProtoMember(8), DefaultValue(0)]
		public int Priority { get; set; }

		void Play()
		{
			sound = Sample.Play(Group, true, Looping, Priority);
			sound.Volume = Volume;
			sound.Pan = Pan;
			sound.Resume();
		}

		void Stop()
		{
			sound.Stop();
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
