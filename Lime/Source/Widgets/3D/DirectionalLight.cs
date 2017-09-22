using Yuzu;

namespace Lime
{
	public struct DirectionalLight
	{
		[YuzuMember]
		public Vector3 Direction
		{
			get
			{
				return dir;
			}
			set
			{
				dir = value;
				normalized = dir.Normalized;
			}
		}

		[YuzuMember]
		public Color4 Color
		{ get; set; }

		public Vector3 DirectionNormalized
		{ get { return normalized; } }

		private Vector3 normalized;
		private Vector3 dir;
	}
}
