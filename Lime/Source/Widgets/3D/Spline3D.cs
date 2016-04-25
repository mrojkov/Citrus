using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Spline3D : Node3D
	{
		[ProtoMember(1)]
		public List<Point> Points { get; set; }

		[ProtoMember(2)]
		public bool Closed { get; set; }

		public Spline3D()
		{
			Points = new List<Point>();
		}

		public float CalcLengthRough()
		{
			var length = 0f;
			var segmentCount = GetSegmentCount();
			var scale = GlobalTransform.GetScale(false);
			for (int i = 0; i < segmentCount; i++) {
				length += ((GetPoint(i + 1).Position - GetPoint(i).Position) * scale).Length;
			}
			return length;
		}

		public float CalcLengthAccurate(int approximateCount)
		{
			var length = 0f;
			var segmentCount = GetSegmentCount();
			for (int i = 0; i < segmentCount; i++) {
				length += CalcSegmentLengthAccurate(GetPoint(i), GetPoint(i + 1), approximateCount);
			}
			return length;
		}

		private float CalcSegmentLengthAccurate(Point a, Point b, int approximateCount)
		{
			var length = 0f;
			var prevPosition = a.Position;
			for (int i = 1; i < approximateCount; i++) {
				var currentPosition = Interpolate((float)i / approximateCount, a, b).Translation;
				length += (currentPosition - prevPosition).Length;
				prevPosition = currentPosition;
			}
			length += (b.Position - prevPosition).Length;
			return length;
		}

		public Matrix44 CalcPointTransformInSpaceOf(float amount, Node3D node)
		{
			return CalcPointTransform(amount) * node.GlobalTransform.CalcInverted();
		}

		public Matrix44 CalcPointTransform(float amount)
		{
			var length = 0f;
			var segmentCount = GetSegmentCount();
			var scale = GlobalTransform.GetScale(false);
			for (int i = 0; i < segmentCount; i++) {
				var point1 = GetPoint(i);
				var point2 = GetPoint(i + 1);
				var segmentLength = ((point2.Position - point1.Position) * scale).Length;
				if (length <= amount && amount < length + segmentLength) {
					return Interpolate((amount - length) / segmentLength, point1, point2);
				}
				length += segmentLength;
			}
			return Matrix44.Identity;
		}

		private Matrix44 Interpolate(float amount, Point point1, Point point2)
		{
			var transform = GlobalTransform;
			var position1 = transform.TransformVector(point1.Position);
			var position2 = transform.TransformVector(point2.Position);
			Vector3 position, direction;
			if (point1.InterpolationMode == InterpolationMode.Linear) {
				position = Mathf.Lerp(amount, position1, position2);
				direction = position2 - position1;
			} else {
				var tangent1 = position1 + transform.TransformNormal(point1.TangentA);
				var tangent2 = position2 + transform.TransformNormal(point1.TangentB);
				position = Mathf.BezierSpline(amount, position1, tangent1, tangent2, position2);
				direction = Mathf.BezierTangent(amount, position1, tangent1, tangent2, position2);
			}
			return CalcRotationMatrix(direction, Vector3.UnitY) * Matrix44.CreateTranslation(position);
		}

		private Matrix44 CalcPointTransform(Vector3 position, Vector3 direction)
		{
			return CalcRotationMatrix(direction, Vector3.UnitY) * Matrix44.CreateTranslation(position);
		}

		public int GetSegmentCount()
		{
			return Points.Count > 2 && Closed ?
				Points.Count :
				Points.Count - 1;
		}

		private Point GetPoint(int index)
		{
			return Points[index % Points.Count];
		}

		private static Matrix44 CalcRotationMatrix(Vector3 direction, Vector3 up)
		{
			var forward = direction.Normalized;
			var right = Vector3.CrossProduct(forward, up).Normalized;
			var normal = Vector3.CrossProduct(right, forward).Normalized;
			var matrix = Matrix44.Identity;
			matrix.Forward = forward;
			matrix.Right = right;
			matrix.Up = normal;
			return matrix;
		}

		[ProtoContract]
		public enum InterpolationMode
		{
			[ProtoEnum]
			Linear,

			[ProtoEnum]
			Bezier
		}

		[ProtoContract]
		public class Point
		{
			[ProtoMember(1)]
			public InterpolationMode InterpolationMode { get; set; }

			[ProtoMember(2)]
			public Vector3 Position { get; set; }

			[ProtoMember(3)]
			public Vector3 TangentA { get; set; }

			[ProtoMember(4)]
			public Vector3 TangentB { get; set; }

			public Point()
			{
				InterpolationMode = InterpolationMode.Bezier;
			}
		}
	}
}