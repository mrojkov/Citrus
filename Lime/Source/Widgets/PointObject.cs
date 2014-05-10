using System;
using System.Collections.Generic;
using System.Text;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	[ProtoInclude(101, typeof(SplinePoint))]
	[ProtoInclude(102, typeof(DistortionMeshPoint))]
	public class PointObject : Node
	{
		private Vector2 position;

		[ProtoMember(1)]
		public Vector2 Position { get { return position; } set { position = value; } }

		public float X { get { return position.X; } set { position.X = value; } }
		public float Y { get { return position.Y; } set { position.Y = value; } }

		[ProtoMember(2)]
		public SkinningWeights SkinningWeights { get; set; }
	}
}
