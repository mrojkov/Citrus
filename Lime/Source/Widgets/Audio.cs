using System;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public enum SoundGroup
	{
		[ProtoEnum]
		Master,
		[ProtoEnum]
		Music,
		[ProtoEnum]
		MusicSub0,
		[ProtoEnum]
		MusicSub1,
		[ProtoEnum]
		Ambient,
		[ProtoEnum]
		AmbientSub0,
		[ProtoEnum]
		AmbientSub1,
		[ProtoEnum]
		Sfx,
		[ProtoEnum]
		SfxSub0,
		[ProtoEnum]
		SfxSub1,
		[ProtoEnum]
		SfxSub2,
		[ProtoEnum]
		SfxSub3,
		[ProtoEnum]
		SfxSub4,
		[ProtoEnum]
		SfxSub5,
		[ProtoEnum]
		UI,
		[ProtoEnum]
		UISub0,
		[ProtoEnum]
		UISub1
	};

	[ProtoContract]
	public enum AudioAction
	{
		[ProtoEnum]
		Play,
		[ProtoEnum]
		Stop
	}

	[ProtoContract]
	public enum AudioFlags
	{
		[ProtoEnum]
		Continual = 1,
		[ProtoEnum]
		Streamed = 2,
		[ProtoEnum]
		Looped = 4
	}

	[ProtoContract]
	public class Audio : Node
	{
		[ProtoMember (1)]
		public PersistentSound Sound { get; set; }

		[ProtoMember (2)]
		public AudioFlags Flags { get; set; }

		[ProtoMember (3)]
		public float FadeTime { get; set; }

		[ProtoMember (4)]
		public float Volume { get; set; }

		[ProtoMember (5)]
		public float Pan { get; set; }

		[ProtoMember (6)]
		public AudioAction Action { get; set; }

		[ProtoMember (7)]
		public SoundGroup Group { get; set; }

		[ProtoMember (8)]
		public float Prioriry { get; set; }
	}
}
