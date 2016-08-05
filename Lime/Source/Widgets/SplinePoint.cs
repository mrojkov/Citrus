using System;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class SplinePoint : PointObject
	{
		[ProtoMember(1)]
		public bool Straight { get; set; }

		[ProtoMember(2)]
		public float TangentAngle { get; set; }

		[ProtoMember(3)]
		public float TangentWeight { get; set; }
	}
}