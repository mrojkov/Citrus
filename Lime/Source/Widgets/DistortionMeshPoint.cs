using System;
using System.Text;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class DistortionMeshPoint : PointObject
	{
		[ProtoMember(1)]
		public Color4 Color { get; set; }

		[ProtoMember(2)]
		public Vector2 UV { get; set; }

		public DistortionMeshPoint()
		{
			Color = Color4.White;
		}

		public Vector2 TransformedPosition {
			get {
				Vector2 result = Vector2.Zero;
				if (Parent != null && Parent.Widget != null)
					result = Vector2.Scale(Parent.Widget.Size, Position);

				if (SkinningWeights != null && Parent != null && Parent.Parent != null) {
					BoneArray a = Parent.Parent.Widget.BoneArray;
					Matrix32 m1 = Parent.Widget.LocalMatrix;
					Matrix32 m2 = m1.CalcInversed();
					result = m2.TransformVector(a.ApplySkinningToVector(m1.TransformVector(result), SkinningWeights));
				}

				return result;
			}
		}
	}
}
