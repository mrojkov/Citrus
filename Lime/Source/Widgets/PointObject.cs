using ProtoBuf;
using Yuzu;

namespace Lime
{
	[ProtoContract]
	[ProtoInclude(101, typeof(SplinePoint))]
	[ProtoInclude(102, typeof(DistortionMeshPoint))]
	[ProtoInclude(103, typeof(EmitterShapePoint))]
	public class PointObject : Node
	{
		private Vector2 position;

		public PointObject()
		{
			// Just in sake of optimization set Presenter to null because all of PointObjects have empty Render() methods.
			Presenter = null;
		}

		[ProtoMember(1)]
		[YuzuMember]
		public Vector2 Position { get { return position; } set { position = value; } }

		public float X { get { return position.X; } set { position.X = value; } }

		public float Y { get { return position.Y; } set { position.Y = value; } }

		[ProtoMember(2)]
		[YuzuMember]
		public SkinningWeights SkinningWeights { get; set; }
	}
}
