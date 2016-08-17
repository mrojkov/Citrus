using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// —плайн. ¬спомогательна€ крива€ лини€, построенна€ по алгоритму Catmull-Rom, задающа€ траекторию дл€ чего-либо
	/// </summary>
	public class Spline : Widget
	{
		/// <summary>
		/// ¬озвращает длину сплайна в пиксел€х (быстро, но грубо)
		/// </summary>
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

		/// <summary>
		/// –аспредел€ет точки равномерно по сплайну. ¬озвращает список координат точек
		/// </summary>
		/// <param name="approximateCount"> оличество точек, которые нужно распределить</param>
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

		/// <summary>
		/// ¬озвращает длину сплайна в пиксел€х (точно, но медленно).
		/// —плайн разбиваетс€ на много мелких отрезков, после чего возвращаетс€ сумма их длинн
		/// </summary>
		/// <param name="approximateCount"> оличество точек аппроксимации. „ем больше точек, тем больше отрезков будет построено и тем точнее будет результат</param>
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

		/// <summary>
		/// ¬озвращает точку на сплайне
		/// </summary>
		/// <param name="lengthFromBeginning">ѕозици€ на сплайне. 0 - начало, 1 - конец</param>
		public Vector2 CalcPoint(float lengthFromBeginning)
		{
			float length = 0.0f;
			SplinePoint p = null;
			foreach (Node node in Nodes) {
				SplinePoint v = node as SplinePoint;
				if (v == null)
					continue;
				if (lengthFromBeginning < 0.0f)
					return v.Position * Size;
				if (p != null) {
					float segmentLength = ((v.Position - p.Position) * Size).Length;
					if (length <= lengthFromBeginning && lengthFromBeginning < length + segmentLength) {
						return Interpolate(p, v, (lengthFromBeginning - length) / segmentLength);
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