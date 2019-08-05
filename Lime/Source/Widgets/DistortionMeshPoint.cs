using Yuzu;

namespace Lime
{
	[TangerineForbidCopyPaste]
	[TangerineAllowedParentTypes(typeof(DistortionMesh))]
	public class DistortionMeshPoint : PointObject
	{
		private Vector2 offset;

		[YuzuMember]
		public Color4 Color { get; set; }

		[YuzuMember]
		public Vector2 UV { get; set; }

		[YuzuMember]
		public override Vector2 Offset
		{
			get => offset;
			set
			{
				if (offset != value) {
					DirtyMask |= DirtyFlags.LocalTransform | DirtyFlags.ParentBoundingRect;
					offset = value;
				}
			}
		}
		
		public DistortionMeshPoint()
		{
			Color = Color4.White;
		}
	}
}
