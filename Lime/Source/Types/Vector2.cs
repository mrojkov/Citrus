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
		public static readonly Vector2 North = new Vector2(0, -1);
		public static readonly Vector2 South = new Vector2(0, 1);
		public static readonly Vector2 East = new Vector2(1, 0);
		public static readonly Vector2 West = new Vector2(-1, 0);
		public static readonly Vector2 PositiveInfinity = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
		public static readonly Vector2 NegativeInfinity = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
		public static readonly Vector2 Up = new Vector2(0, -1);
		public static readonly Vector2 Down = new Vector2(0, 1);
		public static readonly Vector2 Left = new Vector2(-1, 0);
		public static readonly Vector2 Right = new Vector2(1, 0);

		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public static explicit operator IntVector2(Vector2 v)
		{
			return new IntVector2((int)v.X, (int)v.Y);
		}

		public static explicit operator Vector3(Vector2 v)
		{
			return new Vector3(v.X, v.Y, 0);
		}

		public static explicit operator Size(Vector2 v)
		{
			return new Size((int)v.X, (int)v.Y);
		}

		public bool Equals(Vector2 rhs)
		{
			return X == rhs.X && Y == rhs.Y;
		}

		public override bool Equals(object o)
		{
			Vector2 rhs = (Vector2)o;
			return X == rhs.X && Y == rhs.Y;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public static bool operator == (Vector2 lhs, Vector2 rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		public static bool operator != (Vector2 lhs, Vector2 rhs)
		{
			return lhs.X != rhs.X || lhs.Y != rhs.Y;
		}

		public static float AngleDeg(Vector2 a, Vector2 b)
		{
			return AngleRad(a, b) * Mathf.RadiansToDegrees;
		}

		public static float AngleRad(Vector2 a, Vector2 b)
		{
			float sin = a.X * b.Y - b.X * a.Y;
			float cos = a.X * b.X + a.Y * b.Y;
			return (float)Math.Atan2(sin, cos);
		}

		public static Vector2 Lerp(float t, Vector2 a, Vector2 b)
		{
			Vector2 r = new Vector2();
			r.X = (b.X - a.X) * t + a.X;
			r.Y = (b.Y - a.Y) * t + a.Y;
			return r;
		}

		public static float Distance(Vector2 a, Vector2 b)
		{
			return (a - b).Length;
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

        public void Normalize()
        {
            float length = this.Length;
            if (length > 0) {
                this.X /= length;
                this.Y /= length;
            }
        }

		public static Vector2 HeadingDeg(float degrees)
		{
			return Mathf.CosSin(degrees * Mathf.DegreesToRadians);
		}

		public static Vector2 HeadingRad(float radians)
		{
			return Mathf.CosSin(radians);
		}

		public static Vector2 RotateDeg(Vector2 v, float degrees)
		{
			return RotateRad(v, degrees * Mathf.DegreesToRadians);
		}

		public static Vector2 RotateRad(Vector2 v, float radians)
		{
			Vector2 cs = Mathf.CosSin(radians);
			Vector2 result;
			result.X = v.X * cs.X - v.Y * cs.Y;
			result.Y = v.X * cs.Y + v.Y * cs.X;
			return result;
		}

		/// <summary>
		/// Returns ATan of given vector in range (-Pi, Pi]
		/// </summary>
		public float Atan2Rad
		{
			get { return (float)Math.Atan2(Y, X); }
		}

		/// <summary>
		/// Returns ATan of given vector in range (-180, 180]
		/// </summary>
		public float Atan2Deg
		{
			get { return (float)Math.Atan2(Y, X) * Mathf.RadiansToDegrees; }
		}

		public float Length
		{
			get { return (float)Math.Sqrt(X * X + Y * Y); }
		}

		public Vector2 Normalized
		{
			get 
			{
				var v = new Vector2(X, Y);
				v.Normalize();
				return v;
			}
		}

		public float SqrLength
		{
			get { return X * X + Y * Y; }
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}", X, Y);
		}
	}
}