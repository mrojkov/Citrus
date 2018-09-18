using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(Order = 16)]
	[TangerineAllowedParentTypes(typeof(Spline))]
	public class SplinePoint : PointObject
	{
		[YuzuMember]
		[TangerineKeyframeColor(11)]
		public bool Straight { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(12)]
		public float TangentAngle { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(13)]
		public float TangentWeight { get; set; }

		public SplinePoint()
		{
			TangentWeight = 1.0f;
		}
	}
}
