using System;

namespace Lime
{
	public static class GeometryUtils
	{
		public static int CalcPointHalfPlane(Vector2 point, Vector2 lineA, Vector2 lineB)
		{
			float s = (lineB.X - lineA.X) * (point.Y - lineA.Y) - (lineB.Y - lineA.Y) * (point.X - lineA.X);
			if (float.IsNaN(s)) {
				return 0;
			}
			return Math.Sign(s);
		}

		public static bool IntersectLineCircle(Vector2 location, float radius, Vector2 lineFrom, Vector2 lineTo, out Vector2 intersection)
		{
			float ab2, acab, h2;
			Vector2 ac = location - lineFrom;
			Vector2 ab = lineTo - lineFrom;
			ab2 = Vector2.DotProduct(ab, ab);
			acab = Vector2.DotProduct(ac, ab);
			float t = acab / ab2;
			if (t < 0)
				t = 0;
			else if (t > 1)
				t = 1;
			intersection = (ab * t) + lineFrom;
			Vector2 h = intersection - location;
			h2 = Vector2.DotProduct(h, h);
			return h2 <= (radius * radius);
		}

		public static bool CalcLinesIntersection(
			Vector2 a1, Vector2 b1,
			Vector2 a2, Vector2 b2,
			out Vector2 intersection
		) {
			intersection = Vector2.NaN;
			float d = (b2.Y - a2.Y) * (b1.X - a1.X) - (b2.X - a2.X) * (b1.Y - a1.Y);
			float nA = (b2.X - a2.X) * (a1.Y - a2.Y) - (b2.Y - a2.Y) * (a1.X - a2.X);
			float nB = (b1.X - a1.X) * (a1.Y - a2.Y) - (b1.Y - a1.Y) * (a1.X - a2.X);
			if (d == 0) {
				return false;
			}
			float ua = nA / d;
			float ub = nB / d;
			if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1) {
				intersection.X = a1.X + (ua * (b1.X - a1.X));
				intersection.Y = a1.Y + (ua * (b1.Y - a1.Y));
				return true;
			}
			return false;
		}
	}
}
