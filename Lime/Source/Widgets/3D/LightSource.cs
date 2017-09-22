using Yuzu;

namespace Lime
{
	public class LightSource : Node3D
	{
		[YuzuMember]
		public float Intensity
		{ get; set; } = 1f;
	}
}
