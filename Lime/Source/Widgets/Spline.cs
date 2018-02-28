using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	[AllowedChildrenTypes(typeof(SplinePoint))]
	public class Spline : Widget
	{
		[YuzuMember]
		public bool Closed { get; set; }

		public Spline()
		{
			if (!Application.IsTangerine) {
				RenderChainBuilder = null;
			}
		}

		private SplinePoint GetPoint(int index)
		{
			return (SplinePoint)Nodes[index % Nodes.Count];
		}

		public float CalcLengthRough()
		{
			float length = 0;
			var segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				length += ((GetPoint(i + 1).Position - GetPoint(i).Position) * Size).Length;
			}
			return length;
		}

		public int GetSegmentCount()
		{
			if (Nodes.Count > 1) {
				return Closed ? Nodes.Count : Nodes.Count - 1;
			}
			return 0;
		}

		public List<Vector2> ApproximateByPolyline(int approximateCount)
		{
			var result = new List<Vector2>();
			var segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				for (int j = 1; j < approximateCount; j++) {
					Vector2 position = Interpolate(GetPoint(i), GetPoint(i + 1), (float)(j) / approximateCount);
					result.Add(position);
				}
				result.Add(GetPoint(i + 1).Position);
			}
			return result;
		}

		private float CalcSegmentLengthAccurate(SplinePoint point1, SplinePoint point2, int approximateCount)
		{
			float length = 0;
			Vector2 prevPosition = point1.Position * Size;
			for (int i = 1; i < approximateCount; i++) {
				Vector2 curPosition = Interpolate(point1, point2, (float)(i) / approximateCount);
				length += (curPosition - prevPosition).Length;
				prevPosition = curPosition;
			}
			length += (point2.Position * Size - prevPosition).Length;
			return length;
		}

		public float CalcLengthAccurate(int approximateCount)
		{
			float length = 0;
			var segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				length += CalcSegmentLengthAccurate(GetPoint(i), GetPoint(i + 1), approximateCount);
			}
			return length;
		}

		public Vector2 CalcPoint(float lengthFromBeginnning)
		{
			float segStart = 0;
			var segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				var start = GetPoint(i);
				var end = GetPoint(i + 1);
				if (lengthFromBeginnning < 0)
					return start.Position * Size;

				float segLength = ((end.Position - start.Position) * Size).Length;
				if (segStart <= lengthFromBeginnning && lengthFromBeginnning < segStart + segLength) {
					return Interpolate(start, end, (lengthFromBeginnning - segStart) / segLength);
				}
				segStart += segLength;
			}

			return Nodes.Count > 0 ? GetPoint(segmentCount % Nodes.Count).Position * Size : Vector2.Zero;
		}

		public Vector2 CalcNormal(float lengthFromBeginnning)
		{
			float segStart = 0;
			var segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				var start = GetPoint(i);
				var end = GetPoint(i + 1);
				if (lengthFromBeginnning < 0)
					return start.Position * Size;

				float segLength = ((end.Position - start.Position) * Size).Length;
				if (segStart <= lengthFromBeginnning && lengthFromBeginnning < segStart + segLength) {
					return InterpolateNormal(start, end, (lengthFromBeginnning - segStart) / segLength);
				}
				segStart += segLength;
			}

			return Nodes.Count > 0 ? GetPoint(segmentCount % Nodes.Count).Position * Size : Vector2.Zero;
		}

		public IEnumerable<Vector2> CalcPoints(float step)
		{
			float segStart = 0;
			float totalLength = 0;
			var segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				var start = GetPoint(i);
				var end = GetPoint(i + 1);
				float segLength = ((end.Position - start.Position) * Size).Length;
				while (totalLength < segStart + segLength) {
					var t = (totalLength - segStart) / segLength;
					yield return Interpolate(start, end, t);
					totalLength += step;
				}
				segStart += segLength;
			}
			if (Nodes.Count > 0) {
				yield return GetPoint(segmentCount % Nodes.Count).Position* Size;
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

		private Vector2 InterpolateNormal(SplinePoint v1, SplinePoint v2, float t)
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
				return Mathf.HermiteSplineDerivative(t, p1, t1, p2, t2);
			} else {
				Vector2 p1 = v1.Position * Size;
				Vector2 p2 = v2.Position * Size;
				return p1 + t * (p2 - p1);
			}
		}

		public float CalcSplineLengthToNearestPoint(Vector2 point)
		{
			float length = 0;

			float minDistance = float.MaxValue;
			float offset = 0;
			var segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				var start = GetPoint(i);
				var end = GetPoint(i + 1);
				float segmentLength = ((end.Position - start.Position) * Size).Length;
				float minDistance_, offset_;
				if (CalcSplineLengthToNearestPointHelper(start, end, point, out minDistance_, out offset_)) {
					if (minDistance_ < minDistance) {
						offset = offset_ * segmentLength + length;
						minDistance = minDistance_;
					}
					length += segmentLength;
				}
			}
			return offset;
		}

		private bool CalcSplineLengthToNearestPointHelper(SplinePoint v1, SplinePoint v2, Vector2 point, out float minDistance, out float offset)
		{
			const int InnerSegmentCount = 10;
			float ta = 0;
			Vector2 a = Interpolate( v1, v2, ta );
			minDistance = float.MaxValue;
			offset = 0;
			for (int i = 0; i < InnerSegmentCount; ++i) {
				float tb = (float)(i + 1) / InnerSegmentCount;
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