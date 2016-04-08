using System;
using System.Collections.Generic;

namespace Lime
{
	public class Spline3D : Node3D
	{
		public List<SplinePoint3D> Points = new List<SplinePoint3D>();

		public float CalcLengthRough()
		{
			var length = 0f;
			for (int i = 0; i < Points.Count - 1; i++) {
				length += (Points[i + 1].Position - Points[i].Position).Length;
			}
			return length;
		}

		public Vector3 GetPoint(float amount)
		{
			var length = 0f;
			for (int i = 0; i < Points.Count - 1; i++) {
				var a = Points[i];
				var b = Points[i + 1];
				var segmentLength = (b.Position - a.Position).Length;
				if (length <= amount && amount < length + segmentLength) {
					return Interpolate((amount - length) / segmentLength, a, b);
				}
				length += segmentLength;
			}
			return Points.Count > 0 ?
				Points[Points.Count - 1].Position : Vector3.Zero;
		}

		private Vector3 Interpolate(float amount, SplinePoint3D a, SplinePoint3D b)
		{
			var p1 = a.Position;
			var p2 = b.Position;
			if (a.Straight) {
				return Mathf.Lerp(amount, p1, p2);
			}
			var len = (p2 - p1).Length;
			var t1 = p1 + a.TangentB;
			var t2 = p2 + b.TangentA;
			return Mathf.CubicBezierSpline(amount, p1, t1, t2, p2);
		}
	}

	public class SplinePoint3D
	{
		public bool Straight;
		public Vector3 Position;
		public Vector3 TangentA;
		public Vector3 TangentB;
	}
}