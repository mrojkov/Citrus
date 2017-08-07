using System;

namespace Lime
{
	public struct Quadrangle
	{
		public Vector2 V1;
		public Vector2 V2;
		public Vector2 V3;
		public Vector2 V4;

		public Vector2 this[int index]
		{
			get
			{
				if (index == 0) return V1;
				if (index == 1) return V2;
				if (index == 2) return V3;
				if (index == 3) return V4;
				throw new IndexOutOfRangeException();
			}
			set
			{
				switch (index) {
					case 0: V1 = value; break;
					case 1: V2 = value; break;
					case 2: V3 = value; break;
					case 3: V4 = value; break;
					default: throw new IndexOutOfRangeException();
				}
			}
		}

		/// <summary>
		/// Returns true if the quadrangles have common points.
		/// </summary>
		public bool Overlaps(Quadrangle quadrangle)
		{
			for (int k = 0; k < 2; k++) {
				var outerQuad = k == 0 ? this : quadrangle;
				var innerQuad = k == 0 ? quadrangle : this;
				var sign = Vector2.CrossProduct(outerQuad.V2 - outerQuad.V1, outerQuad.V4 - outerQuad.V1);
				if (sign.Abs() < Mathf.ZeroTolerance) {
					return false;
				}
				for (int i = 0; i < 4; i++) {
					var a = outerQuad[i];
					var b = outerQuad[(i + 1) % 4];
					var allOutside = true;
					for (int j = 0; j < 4; j++) {
						allOutside &= GeometryUtils.CalcPointHalfPlane(innerQuad[j], a, b) * sign < 0;
					}
					if (allOutside) {
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Returns true if one side of the first quadrangle intersects a side of the second quadrangle.
		/// </summary>
		public bool Intersects(Quadrangle quadrangle)
		{
			for (int i = 0; i < 4; i++) {
				var a1 = this[i];
				var b1 = this[(i + 1) % 4];
				for (int j = 0; j < 4; j++) {
					var a2 = quadrangle[j];
					var b2 = quadrangle[(j + 1) % 4];
					Vector2 intersection;
					if (GeometryUtils.CalcLinesIntersection(a1, b1, a2, b2, out intersection)) {
						return true;
					}
				}
			}
			return false;
		}

		public static Quadrangle operator *(Quadrangle q, Vector2 v)
		{
			return new Quadrangle {
				V1 = q.V1 * v,
				V2 = q.V2 * v,
				V3 = q.V3 * v,
				V4 = q.V4 * v,
			};
		}

		public static Quadrangle operator *(Quadrangle q, Matrix32 m)
		{
			return new Quadrangle {
				V1 = q.V1 * m,
				V2 = q.V2 * m,
				V3 = q.V3 * m,
				V4 = q.V4 * m,
			};
		}

		public bool Contains(Vector2 point)
		{
			var sign = Vector2.CrossProduct(V2 - V1, V4 - V1);
			var inside = true;
			for (int i = 0; i < 4; i++) {
				var a = this[i];
				var b = this[(i + 1) % 4];
				inside &= GeometryUtils.CalcPointHalfPlane(point, a, b) * sign > 0;
			}
			return inside;
		}
	}
}