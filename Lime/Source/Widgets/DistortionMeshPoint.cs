using Yuzu;

namespace Lime
{
	public class DistortionMeshPoint : PointObject
	{
		[YuzuMember]
		public Color4 Color { get; set; }

		[YuzuMember]
		public Vector2 UV { get; set; }

		public DistortionMeshPoint()
		{
			Color = Color4.White;
		}
	}
}
