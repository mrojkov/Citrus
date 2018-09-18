using Yuzu;

namespace Lime
{
	[TangerineForbidCopyPaste]
	[TangerineAllowedParentTypes(typeof(DistortionMesh))]
	public class DistortionMeshPoint : PointObject
	{
		[YuzuMember]
		public Color4 Color { get; set; }

		[YuzuMember]
		public Vector2 UV { get; set; }

		[YuzuMember]
		public override Vector2 Offset { get; set; }

		public DistortionMeshPoint()
		{
			Color = Color4.White;
		}
	}
}
