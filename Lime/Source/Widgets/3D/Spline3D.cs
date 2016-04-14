using System;
using System.Collections.Generic;

namespace Lime
{
	public class Spline3D : Node3D
	{
		public List<SplinePoint3D> Points = new List<SplinePoint3D>();

		public bool Closed { get; set; }

		public float CalcLengthRough()
		{
			return CalcLengthRough(Scale);
		}

		public float CalcLengthRough(Matrix44 transform)
		{
			return CalcLengthRough(transform.GetScale(false));
		}

		public float CalcLengthRough(Vector3 scale)
		{
			var length = 0f;
			var segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				length += ((Points[(i + 1) % Points.Count].Position - Points[i].Position) * scale).Length;
			}
			return length;
		}

		public float CalcLengthAccurate(int approximateCount)
		{
			return CalcLengthAccurate(Scale, approximateCount);
		}

		public float CalcLengthAccurate(Matrix44 transform, int approximateCount)
		{
			return CalcLengthAccurate(transform.GetScale(false), approximateCount);
		}

		public float CalcLengthAccurate(Vector3 scale, int approximateCount)
		{
			var length = 0f;
			var segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				length += CalcSegmentLengthAccurate(Points[i], Points[(i + 1) % Points.Count], scale, approximateCount);
			}
			return length;
		}

		private float CalcSegmentLengthAccurate(SplinePoint3D a, SplinePoint3D b, Vector3 scale, int approximateCount)
		{
			var length = 0f;
			var prevPosition = a.Position;
			for (int i = 1; i < approximateCount; i++) {
				var currentPosition = Interpolate((float)i / approximateCount, a, b);
				length += ((currentPosition - prevPosition) * scale).Length;
				prevPosition = currentPosition;
			}
			length += ((b.Position - prevPosition) * scale).Length;
			return length;
		}

		public Vector3 GetPoint(float amount)
		{
			return GetPoint(amount, CalcLocalTransform());
		}

		public Vector3 GetPoint(float amount, Matrix44 transform)
		{
			var length = 0f;
			var scale = transform.GetScale(false);
			int segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				var a = Points[i];
				var b = Points[(i + 1) % Points.Count];
				var segmentLength = ((b.Position - a.Position) * scale).Length;
				if (length <= amount && amount < length + segmentLength) {
					return transform.TransformVector(Interpolate((amount - length) / segmentLength, a, b));
				}
				length += segmentLength;
			}
			return Points.Count > 0 ?
				transform.TransformVector(Points[Points.Count - 1].Position) : Vector3.Zero;
		}

		private Vector3 Interpolate(float amount, SplinePoint3D a, SplinePoint3D b)
		{
			var p1 = a.Position;
			var p2 = b.Position;
			var t1 = p1 + a.TangentA;
			var t2 = p2 + b.TangentB;
			switch (a.InterpolationMode) {
				case SplineInterpolationMode3D.Linear:
					return Mathf.Lerp(amount, p1, p2);
				case SplineInterpolationMode3D.Bezier:
					return Mathf.BezierSpline(amount, p1, t1, t2, p2);
				default:
					throw new InvalidOperationException("Invalid spline interpolation mode");
			}
		}

		public int GetSegmentCount()
		{
			return Points.Count > 2 && Closed ? Points.Count : Points.Count - 1;
		}
	}

	public class SplinePoint3D
	{
		public SplineInterpolationMode3D InterpolationMode = SplineInterpolationMode3D.Bezier;
		public Vector3 Position;
		public Vector3 TangentA;
		public Vector3 TangentB;

		public float TangentWeightA
		{
			get { return TangentB.Length; }
			set { TangentB = TangentB.Normalized * value; }
		}

		public float TangentWeightB
		{
			get { return TangentA.Length; }
			set { TangentA = TangentA.Normalized * value; }
		}
	}

	public enum SplineInterpolationMode3D
	{
		Linear,
		Bezier
	}
}