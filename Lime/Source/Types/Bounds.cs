using System;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// An axis-aligned bounding box, or AABB for short, is a box aligned with coordinate axes and fully enclosing some object.
	/// </summary>
	[YuzuCompact]
	public struct Bounds : IEquatable<Bounds>
	{
		[YuzuMember("0")]
		public Vector3 A;

		[YuzuMember("1")]
		public Vector3 B;

		public static readonly Bounds Empty = new Bounds();

		public Bounds(float left, float right, float bottom, float top, float back, float front)
		{
			A.X = left;
			A.Y = bottom;
			A.Z = back;
			B.X = right;
			B.Y = top;
			B.Z = front;
		}

		public Bounds(Vector3 a, Vector3 b)
		{
			A = a;
			B = b;
		}

		public float Width
		{
			get { return B.X - A.X; }
			set { B.X = A.X + value; }
		}

		public float Height
		{
			get { return B.Y - A.Y; }
			set { B.Y = A.Y + value; }
		}

		public float Depth
		{
			get { return B.Z - A.Z; }
			set { B.Z = A.Z + value; }
		}

		public float Left { get { return A.X; } set { A.X = value; } }
		public float Right { get { return B.X; } set { B.X = value; } }
		public float Bottom { get { return A.Y; } set { A.Y = value; } }
		public float Top { get { return B.Y; } set { B.Y = value; } }
		public float Back { get { return A.Z; } set { A.Z = value; } }
		public float Front { get { return B.Z; } set { B.Z = value; } }

		public Vector3 Center => (A + B) * 0.5f;
		public Vector3 Size => B - A;

		public Bounds Normalized
		{
			get
			{
				var rect = this;
				if (Width < 0) {
					rect.A.X = B.X;
					rect.B.X = A.X;
				}
				if (Height < 0) {
					rect.A.Y = B.Y;
					rect.B.Y = A.Y;
				}
				if (Depth < 0) {
					rect.A.Z = B.Z;
					rect.B.Z = A.Z;
				}
				return rect;
			}
		}

		public bool Contains(Vector3 value)
		{
			return (value.X >= A.X) && (value.Y >= A.Y) && (value.Z >= A.Z) && (value.X < B.X) && (value.Y < B.Y) && (value.Z < B.Z);
		}

		public static Bounds Intersect(Bounds value1, Bounds value2)
		{
			var x0 = Math.Max(value1.A.X, value2.A.X);
			var x1 = Math.Min(value1.B.X, value2.B.X);
			var y0 = Math.Max(value1.A.Y, value2.A.Y);
			var y1 = Math.Min(value1.B.Y, value2.B.Y);
			var z0 = Math.Max(value1.A.Z, value2.A.Z);
			var z1 = Math.Min(value1.B.Z, value2.B.Z);
			return x1 >= x0 && y1 >= y0 && z1 >= z0 ? new Bounds(x0, x1, y0, y1, z0, z1) : Empty;
		}

		public static Bounds Combine(Bounds value1, Bounds value2)
		{
			return new Bounds(
				Mathf.Min(value1.Left, value2.Left),
				Mathf.Max(value1.Right, value2.Right),
				Mathf.Min(value1.Bottom, value2.Bottom),
				Mathf.Max(value1.Top, value2.Top),
				Mathf.Min(value1.Back, value2.Back),
				Mathf.Max(value1.Front, value2.Front)
			);
		}

		public Bounds IncludingPoint(Vector3 value)
		{
			return new Bounds(
				Mathf.Min(value.X, Left),
				Mathf.Max(value.X, Right),
				Mathf.Min(value.Y, Bottom),
				Mathf.Max(value.Y, Top),
				Mathf.Min(value.Z, Back),
				Mathf.Max(value.Z, Front)
			);
		}

		public override string ToString()
		{
			return $"{A.X}, {A.Y}, {A.Z}, {B.X}, {B.Y}, {B.Z}";
		}

		public override int GetHashCode()
		{
			unchecked {
				return (A.GetHashCode() * 397) ^ B.GetHashCode();
			}
		}

		public Bounds Transform(Matrix44 value)
		{
			return new Bounds(A * value, B * value);
		}

		public override bool Equals(object obj)
		{
			return obj is Bounds && Equals((Bounds)obj);
		}

		public bool Equals(Bounds other)
		{
			return A.Equals(other.A) && B.Equals(other.B);
		}

		public static bool operator ==(Bounds lhs, Bounds rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Bounds lhs, Bounds rhs)
		{
			return !lhs.Equals(rhs);
		}
	}
}
