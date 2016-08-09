using ProtoBuf;
using Yuzu;

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
		[YuzuMember]
		public SerializableSample Sample { get; set; }

		/// <summary>
		/// Зацикленное проигрывание
		/// </summary>
		[ProtoMember(2)]
		[YuzuMember]
		public bool Looping { get; set; }

		/// <summary>
		/// Время затухания в секундах
		/// </summary>
		[ProtoMember(3)]
		[YuzuMember]
		public float FadeTime { get; set; }

		private float volume = 0.5f;

		/// <summary>
		/// Громкость (0 - 1)
		/// </summary>
		[ProtoMember(4)]
		[YuzuMember]
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

		/// <summary>
		/// Сдвиг влево/вправо (-1 - влево, 1 - вправо, 0 - посередине)
		/// </summary>
		[ProtoMember(5)]
		[YuzuMember]
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

		/// <summary>
		/// Высота звука
		/// </summary>
		[ProtoMember(9)]
		[YuzuMember]
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

		/// <summary>
		/// Группа звуков. Например голос, фоновый звук, эффект. Группа позволяет задавать общие параметры для всех членов группы
		/// </summary>
		[ProtoMember(7)]
		[YuzuMember]
		public AudioChannelGroup Group { get; set; }

		[ProtoMember(10)]
		[YuzuMember]
		public float Priority { get; set; }

		[ProtoMember(11)]
		[YuzuMember]
		public bool Bumpable { get; set; }

		public Audio()
		{
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

		protected override void SelfUpdate(float delta)
		{
			sound.Bump();
		}

		public override void OnTrigger(string property)
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
