using System;
using ProtoBuf;
using Yuzu;

namespace Lime
{
	[ProtoContract]
	public enum MarkerAction
	{
		[ProtoEnum]
		Play,
		[ProtoEnum]
		Stop,
		[ProtoEnum]
		Jump,
		[ProtoEnum]
		Destroy
	}

	[ProtoContract]
	public class Marker
	{
		[ProtoMember(1)]
		[YuzuMember]
		public string Id { get; set; }

		[ProtoMember(2)]
		[YuzuMember]
		public int Frame { get; set; }

		public int Time { get { return AnimationUtils.FramesToMsecs(Frame); } }

		[ProtoMember(3)]
		[YuzuMember]
		public MarkerAction Action { get; set; }

		[ProtoMember(4)]
		[YuzuMember]
		public string JumpTo { get; set; }

		public Action CustomAction { get; set; }

		public Marker() { }

		public Marker(string id, int frame, MarkerAction action, string jumpTo = null)
		{
			this.Id = id;
			this.Frame = frame;
			this.Action = action;
			this.JumpTo = jumpTo;
		}

		public Marker Clone()
		{
			return (Marker)MemberwiseClone();
		}

		public override string ToString()
		{
			return string.Format("{1} '{0}'", Id, Action);
		}
	}
}