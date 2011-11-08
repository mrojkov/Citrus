using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Spline : Widget
	{
		public Spline ()
		{
		}

		public float CalcLength()
		{
			float length = 0;
			SplinePoint p = null;
			foreach (Node node in Nodes) {
				SplinePoint v = node as SplinePoint;
				if (v == null)
					continue;
				if (p != null) {
					length += ((v.Position - p.Position) * Size).Length;
				}
				p = v;
			}
			return length;
		}

		public Vector2 CalcPoint (float t)
		{
			float length = 0.0f;
			SplinePoint p = null;
			foreach (Node node in Nodes) {
				SplinePoint v = node as SplinePoint;
				if (v == null)
					continue;
				if (t < 0.0f)
					return v.Position * Size;
				if (p != null) {
					float segmentLength = ((v.Position - p.Position) * Size).Length;
					if (length <= t && t < length + segmentLength) {
						return Interpolate (p, v, (t - length) / segmentLength);
					}
					length += segmentLength;
				}
				p = v;
			}
			if (p != null)
				return p.Position * Size;
			else
				return Vector2.Zero;
		}

		Vector2 Interpolate (SplinePoint v1, SplinePoint v2, float t)
		{
			if( !v1.Straight ) {
				Vector2 p1 = v1.Position * Size;
				Vector2 p2 = v2.Position * Size;
				float len = (p2 - p1).Length;
				float ta1 = v1.TangentAngle * Utils.DegreesToRadians;
				float ta2 = v2.TangentAngle * Utils.DegreesToRadians;
				Vector2 t1 = Vector2.CosSin (ta1);
				Vector2 t2 = Vector2.CosSin (ta2);
				t1 *= len * v1.TangentWeight;
				t2 *= len * v2.TangentWeight;
				return Utils.HermiteSpline( t, p1, t1, p2, t2 );
			} else {
				Vector2 p1 = v1.Position * Size;
				Vector2 p2 = v2.Position * Size;
				return p1 + t * (p2 - p1);
			}
		}

		float CalcOffset (Vector2 point)
		{
			float length = 0;
			SplinePoint p = null;
			float minDistance = float.MaxValue;
			float offset = 0;
			foreach (Node node in Nodes) {
				SplinePoint v = node as SplinePoint;
				if(v == null)
					continue;
				if (p != null) {
					float segmentLength = ((v.Position - p.Position) * Size).Length;
					float minDistance_, offset_;
					if (CalcOffsetHelper (p, v, point, out minDistance_, out offset_)) {
						if (minDistance_ < minDistance) {
							offset = offset_ * segmentLength + length;
							minDistance = minDistance_;
						}
					}
					length += segmentLength;
				}
				p = v;
			}
			return offset;
		}
	
		bool CalcOffsetHelper (SplinePoint v1, SplinePoint v2, Vector2 point, out float minDistance, out float offset)
		{
			const int SegmentCount = 10;
			float ta = 0;
			Vector2 a = Interpolate( v1, v2, ta );
			minDistance = float.MaxValue;
			offset = 0;
			for( int i = 0; i < SegmentCount; ++i ) {
				float tb = (float)(i + 1) / SegmentCount;
				Vector2 b = Interpolate (v1, v2, tb);
				float minDistance_, offset_;
				if (CalcLineOffset (a, b, point, out minDistance_, out offset_ )) {
					if( minDistance_ < minDistance ) {
						offset = offset_ * (tb - ta) + ta;
						minDistance = minDistance_;
					}
				}
				a = b;
				ta = tb;
			}
			return minDistance < float.MaxValue;
		}

		bool CalcLineOffset (Vector2 a, Vector2 b, Vector2 point, out float minDistance, out float offset )
		{
			Vector2 v = b - a;
			float len = v.Length;
			if (len < 1e-5) {
				offset = minDistance = 0;
				return false;
			}
			Vector2 direction = v / len;
			float f = Vector2.DotProduct (direction, point - a);
			offset = Utils.Clamp (f / len, 0, 1);
			Vector2 projectedPoint = a + offset * v;
			minDistance = (projectedPoint - point).Length;
			return true;
		}
	}
}