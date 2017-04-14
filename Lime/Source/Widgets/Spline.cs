using System.Collections.Generic;

namespace Lime
{
	[AllowedChildrenTypes(typeof(SplinePoint))]
	public class Spline : Widget
	{
		public float CalcLengthRough()
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

		public List<Vector2> ApproximateByPolyline(int approximateCount)
		{
			var result = new List<Vector2>();
			SplinePoint prevPoint = null;
			foreach (SplinePoint curPoint in Nodes) {
				if (prevPoint != null) {
					for (int i = 1; i < approximateCount; i++) {
						Vector2 position = Interpolate(prevPoint, curPoint, (float)(i) / approximateCount);
						result.Add(position);
					}
				}
				result.Add(curPoint.Position);
				prevPoint = curPoint;
			}
			return result;
		}

		private float CalcSegmentLengthAccurate(SplinePoint point1, SplinePoint point2, int approximateCount)
		{
			float length = 0;
			Vector2 prevPosition = point1.Position;
			for (int i = 1; i < approximateCount; i++) {
				Vector2 curPosition = Interpolate(point1, point2, (float)(i) / approximateCount);
				length += (curPosition - prevPosition).Length;
				prevPosition = curPosition;
			}
			length += (point2.Position - prevPosition).Length;
			return length;
		}

		public float CalcLengthAccurate(int approximateCount)
		{
			float length = 0;
			SplinePoint prevPoint = null;
			foreach (SplinePoint curPoint in Nodes) {
				if (prevPoint != null) {
					length += CalcSegmentLengthAccurate(prevPoint, curPoint, approximateCount);
				}
				prevPoint = curPoint;
			}
			return length;
		}
		
		public Vector2 CalcPoint(float lengthFromBeginnning)
		{
			SplinePoint pre = null;
			float segStart = 0;
			foreach (var node in Nodes) {
				var cur = node as SplinePoint;
				if (cur == null)
					continue;
				if (lengthFromBeginnning < 0)
					return cur.Position * Size;
				if (pre != null) {
					float segLength = ((cur.Position - pre.Position) * Size).Length;
					if (segStart <= lengthFromBeginnning && lengthFromBeginnning < segStart + segLength) {
						return Interpolate(pre, cur, (lengthFromBeginnning - segStart) / segLength);
					}
					segStart += segLength;
				}
				pre = cur;
			}
			return pre != null ? pre.Position * Size : Vector2.Zero;
		}
		
		public IEnumerable<Vector2> CalcPoints(float step)
		{
			SplinePoint pre = null;
			float segStart = 0;
			float totalLength = 0;
			foreach (var node in Nodes) {
				var cur = node as SplinePoint;
				if (cur == null)
					continue;
				if (pre != null) {
					var segLength = ((cur.Position - pre.Position) * Size).Length;
					while (totalLength < segStart + segLength) {
						var t = (totalLength - segStart) / segLength;
						yield return Interpolate(pre, cur, t);
						totalLength += step;
					}
					segStart += segLength;
				}
				pre = cur;
			}
			if (pre != null) {
				yield return pre.Position * Size;
			}
		}

		private Vector2 Interpolate(SplinePoint v1, SplinePoint v2, float t)
		{
			if (!v1.Straight) {
				Vector2 p1 = v1.Position * Size;
				Vector2 p2 = v2.Position * Size;
				float len = (p2 - p1).Length;
				float ta1 = v1.TangentAngle * Mathf.DegToRad;
				float ta2 = v2.TangentAngle * Mathf.DegToRad;
				Vector2 t1 = Vector2.CosSinRough(ta1);
				Vector2 t2 = Vector2.CosSinRough(ta2);
				t1 *= len * v1.TangentWeight;
				t2 *= len * v2.TangentWeight;
				return Mathf.HermiteSpline( t, p1, t1, p2, t2 );
			} else {
				Vector2 p1 = v1.Position * Size;
				Vector2 p2 = v2.Position * Size;
				return p1 + t * (p2 - p1);
			}
		}

		public float CalcSplineLengthToNearestPoint(Vector2 point)
		{
			float length = 0;
			SplinePoint p = null;
			float minDistance = float.MaxValue;
			float offset = 0;
			foreach (Node node in Nodes) {
				SplinePoint v = node as SplinePoint;
				if (v == null)
					continue;
				if (p != null) {
					float segmentLength = ((v.Position - p.Position) * Size).Length;
					float minDistance_, offset_;
					if (CalcSplineLengthToNearestPointHelper(p, v, point, out minDistance_, out offset_)) {
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

		private bool CalcSplineLengthToNearestPointHelper(SplinePoint v1, SplinePoint v2, Vector2 point, out float minDistance, out float offset)
		{
			const int SegmentCount = 10;
			float ta = 0;
			Vector2 a = Interpolate( v1, v2, ta );
			minDistance = float.MaxValue;
			offset = 0;
			for (int i = 0; i < SegmentCount; ++i) {
				float tb = (float)(i + 1) / SegmentCount;
				Vector2 b = Interpolate(v1, v2, tb);
				float minDistance_, offset_;
				if (ProjectPointToLine(a, b, point, out minDistance_, out offset_ )) {
					if (minDistance_ < minDistance) {
						offset = offset_ * (tb - ta) + ta;
						minDistance = minDistance_;
					}
				}
				a = b;
				ta = tb;
			}
			return minDistance < float.MaxValue;
		}

		private bool ProjectPointToLine(Vector2 a, Vector2 b, Vector2 point, out float minDistance, out float offset)
		{
			Vector2 v = b - a;
			float len = v.Length;
			if (len < 1e-5) {
				offset = minDistance = 0;
				return false;
			}
			Vector2 direction = v / len;
			float f = Vector2.DotProduct(direction, point - a);
			offset = Mathf.Clamp(f / len, 0, 1);
			Vector2 projectedPoint = a + offset * v;
			minDistance = (projectedPoint - point).Length;
			return true;
		}
	}
}