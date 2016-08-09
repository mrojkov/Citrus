using ProtoBuf;
using Yuzu;

namespace Lime
{
	[ProtoContract]
	public class SplinePoint : PointObject
	{
		[ProtoMember(1)]
		[YuzuMember]
		public bool Straight { get; set; }

		[ProtoMember(2)]
		[YuzuMember]
		public float TangentAngle { get; set; }

		[ProtoMember(3)]
		[YuzuMember]
		public float TangentWeight { get; set; }
	}
}