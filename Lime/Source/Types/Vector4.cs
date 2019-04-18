using System;
using Yuzu;

namespace Lime
{
	[YuzuCompact]
	public struct Vector4 : IEquatable<Vector4>
	{
		[YuzuMember("0")]
		public float X;

		[YuzuMember("1")]
		public float Y;

		[YuzuMember("2")]
		public float Z;

		[YuzuMember("3")]
		public float W;

		/// <summary>
		/// Returns a vector with components 0, 0, 0, 0.
		/// </summary>
		public static readonly Vector4 Zero = new Vector4(0, 0, 0, 0);

		/// <summary>
		/// Returns a vector with components 1, 1, 1, 1.
		/// </summary>
		public static readonly Vector4 One = new Vector4(1, 1, 1, 1);

		public float Length
		{
			get { return Mathf.Sqrt(SqrLength); }
		}

		public float SqrLength
		{
			get { return X * X + Y * Y + Z * Z + W * W; }
		}

		public Vector4 Normalized
		{
			get
			{
				var v = new Vector4(X, Y, Z, W);
				var length = Length;
				if (length > 0) {
					v /= length;
				}
				return v;
			}
		}

		public Vector4(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public Vector4(Vector3 xyz, float w)
			: this(xyz.X, xyz.Y, xyz.Z, w)
		{
		}

		public override bool Equals(object obj)
		{
			return obj is Vector4 && Equals((Vector4)obj);
		}

		public bool Equals(Vector4 value)
		{
			return this == value;
		}

		public static float DotProduct(Vector4 value1, Vector4 value2)
		{
			return
				value1.X * value2.X +
				value1.Y * value2.Y +
				value1.Z * value2.Z +
				value1.W * value2.W;
		}

		/// <summary>
		/// Gets or sets the vector component by its index.
		/// </summary>
		public float this [int component]
		{
			get
			{
				if (component == 0) {
					return X;
				} else if (component == 1) {
					return Y;
				} else if (component == 2) {
					return Z;
				} else if (component == 3) {
					return W;
				} else {
					throw new IndexOutOfRangeException();
				}
			}
			set
			{
				if (component == 0) {
					X = value;
				} else if (component == 1) {
					Y = value;
				} else if (component == 2) {
					Z = value;
				} else if (component == 3) {
					W = value;
				} else {
					throw new IndexOutOfRangeException();
				}
			}
		}

		public static Vector4 operator -(Vector4 value)
		{
			return new Vector4(-value.X, -value.Y, -value.Z, -value.W);
		}

		public static bool operator ==(Vector4 value1, Vector4 value2)
		{
			return value1.W == value2.W
				&& value1.X == value2.X
				&& value1.Y == value2.Y
				&& value1.Z == value2.Z;
		}

		public static bool operator !=(Vector4 value1, Vector4 value2)
		{
			return !(value1 == value2);
		}

		public static Vector4 operator +(Vector4 value1, Vector4 value2)
		{
			value1.W += value2.W;
			value1.X += value2.X;
			value1.Y += value2.Y;
			value1.Z += value2.Z;
			return value1;
		}

		public static Vector4 operator -(Vector4 value1, Vector4 value2)
		{
			value1.W -= value2.W;
			value1.X -= value2.X;
			value1.Y -= value2.Y;
			value1.Z -= value2.Z;
			return value1;
		}

		public static Vector4 operator *(Vector4 value1, Vector4 value2)
		{
			value1.W *= value2.W;
			value1.X *= value2.X;
			value1.Y *= value2.Y;
			value1.Z *= value2.Z;
			return value1;
		}

		public static Vector4 operator *(Vector4 value, float scaleFactor)
		{
			value.W *= scaleFactor;
			value.X *= scaleFactor;
			value.Y *= scaleFactor;
			value.Z *= scaleFactor;
			return value;
		}

		public static Vector4 operator *(float scaleFactor, Vector4 value)
		{
			value.W *= scaleFactor;
			value.X *= scaleFactor;
			value.Y *= scaleFactor;
			value.Z *= scaleFactor;
			return value;
		}

		public static Vector4 operator /(Vector4 value1, Vector4 value2)
		{
			value1.W /= value2.W;
			value1.X /= value2.X;
			value1.Y /= value2.Y;
			value1.Z /= value2.Z;
			return value1;
		}

		public static Vector4 operator /(Vector4 value1, float divider)
		{
			float factor = 1f / divider;
			value1.W *= factor;
			value1.X *= factor;
			value1.Y *= factor;
			value1.Z *= factor;
			return value1;
		}

		public static explicit operator Vector2(Vector4 value)
		{
			return new Vector2(value.X, value.Y);
		}

		public static explicit operator Vector3(Vector4 value)
		{
			return new Vector3(value.X, value.Y, value.Z);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				hashCode = (hashCode * 397) ^ W.GetHashCode();
				return hashCode;
			}
		}
	}
}
