using System;
using ProtoBuf;

namespace Lime
{
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct Vector2 : IEquatable<Vector2>
	{
		[ProtoMember(1)]
		public float X;

		[ProtoMember(2)]
		public float Y;

		public static readonly Vector2 Zero = new Vector2(0, 0);
		public static readonly Vector2 One = new Vector2(1, 1);
		public static readonly Vector2 Half = new Vector2(0.5f, 0.5f);

		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public static explicit operator IntVector2(Vector2 v)
		{
			return new IntVector2((int)v.X, (int)v.Y);
		}

		public static explicit operator Size(Vector2 v)
		{
			return new Size((int)v.X, (int)v.Y);
		}

		bool IEquatable<Vector2>.Equals(Vector2 rhs)
		{
			return X == rhs.X && Y == rhs.Y;
		}

		public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
		{
			Vector2 r = new Vector2();
			r.X = (b.X - a.X) * t + a.X;
			r.Y = (b.Y - a.Y) * t + a.Y;
			return r;
		}

		public static Vector2 operator *(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X * rhs.X, lhs.Y * rhs.Y);
		}

		public static Vector2 operator /(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X / rhs.X, lhs.Y / rhs.Y);
		}

		public static Vector2 operator /(Vector2 lhs, float rhs)
		{
			return new Vector2(lhs.X / rhs, lhs.Y / rhs);
		}

		public static Vector2 Scale(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X * rhs.X, lhs.Y * rhs.Y);
		}

		public static Vector2 operator *(float lhs, Vector2 rhs)
		{
			return new Vector2(lhs * rhs.X, lhs * rhs.Y);
		}

		public static Vector2 operator *(Vector2 lhs, float rhs)
		{
			return new Vector2(rhs * lhs.X, rhs * lhs.Y);
		}

		public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
		}

		public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
		}

		public static Vector2 operator -(Vector2 v)
		{
			return new Vector2(-v.X, -v.Y);
		}

		public static float DotProduct(Vector2 lhs, Vector2 rhs)
		{
			return lhs.X * rhs.X + lhs.Y * rhs.Y;
		}

		public static float CrossProduct(Vector2 lhs, Vector2 rhs)
		{
			return lhs.X * rhs.Y - lhs.Y * rhs.X;
		}

		public static Vector2 Normalize(Vector2 value)
		{
			float length = value.Length;
			if (length > 0) {
				value.X /= length;
				value.Y /= length;
			}
			return value;
		}

		public static Vector2 CosSin(float radians)
		{
			return CitMath.CosSin(radians);
			// No more slow sine/cosine!
			//return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
		}

		public static Vector2 Rotate(Vector2 value, float radians)
		{
			Vector2 cs = Vector2.CosSin(radians);
			Vector2 result;
			result.X = value.X * cs.X - value.Y * cs.Y;
			result.Y = value.X * cs.Y + value.Y * cs.X;
			return result;
		}

		public float Atan2
		{
			get { return (float)Math.Atan2(Y, X); }
		}

		public float Length
		{
			get { return (float)Math.Sqrt(X * X + Y * Y); }
		}

		public float SquaredLength
		{
			get { return X * X + Y * Y; }
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}", X, Y);
		}
	}
}