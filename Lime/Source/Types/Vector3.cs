using System;
using System.Globalization;
using System.Linq;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Representation of 3D vectors and points.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[YuzuCompact]
	public struct Vector3 : IEquatable<Vector3>
	{
		[YuzuMember("0")]
		public float X;

		[YuzuMember("1")]
		public float Y;

		[YuzuMember("2")]
		public float Z;

		/// <summary>
		/// Returns a vector with components 0, 0, 0.
		/// </summary>
		public static readonly Vector3 Zero = new Vector3(0, 0, 0);

		/// <summary>
		/// Returns a vector with components 1, 1, 1.
		/// </summary>
		public static readonly Vector3 One = new Vector3(1, 1, 1);

		/// <summary>
		/// Returns a vector with components 0.5, 0.5, 0.5.
		/// </summary>
		public static readonly Vector3 Half = new Vector3(0.5f, 0.5f, 0.5f);

		public static readonly Vector3 UnitX = new Vector3(1f, 0f, 0f);
		public static readonly Vector3 UnitY = new Vector3(0f, 1f, 0f);
		public static readonly Vector3 UnitZ = new Vector3(0f, 0f, 1f);

		/// <summary>
		/// Returns a vector with components NaN, NaN, NaN.
		/// </summary>
		public static readonly Vector3 NaN = new Vector3(float.NaN, float.NaN, float.NaN);

		public Vector3(Vector2 xy, float z)
			: this(xy.X, xy.Y, z)
		{
		}

		public Vector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Vector3(float xyz)
		{
			X = xyz;
			Y = xyz;
			Z = xyz;
		}

		public static explicit operator Vector2(Vector3 value)
		{
			return new Vector2(value.X, value.Y);
		}

		public bool Equals(Vector3 other)
		{
			return X == other.X
				&& Y == other.Y
				&& Z == other.Z;
		}

		public override bool Equals(object obj)
		{
			return obj is Vector3 && Equals((Vector3)obj);
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains
		/// linear interpolation of the specified vectors.
		/// </summary>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The result of linear interpolation of the specified vectors.</returns>
		public static Vector3 Lerp(float amount, Vector3 value1, Vector3 value2)
		{
			return new Vector3
			{
				X = Mathf.Lerp(amount, value1.X, value2.X),
				Y = Mathf.Lerp(amount, value1.Y, value2.Y),
				Z = Mathf.Lerp(amount, value1.Z, value2.Z)
			};
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
				} else {
					throw new IndexOutOfRangeException();
				}
			}
		}

		public static Vector3 operator *(Vector3 lhs, Vector3 rhs)
		{
			return new Vector3(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z);
		}

		public static Vector3 operator /(Vector3 lhs, Vector3 rhs)
		{
			return new Vector3(lhs.X / rhs.X, lhs.Y / rhs.Y, lhs.Z / rhs.Z);
		}

		public static Vector3 operator /(Vector3 lhs, float rhs)
		{
			return new Vector3(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);
		}

		public static bool operator ==(Vector3 lhs, Vector3 rhs)
		{
			return lhs.X == rhs.X
				&& lhs.Y == rhs.Y
				&& lhs.Z == rhs.Z;
		}

		public static bool operator !=(Vector3 lhs, Vector3 rhs)
		{
			return !(lhs == rhs);
		}

		public static Vector3 operator *(float lhs, Vector3 rhs)
		{
			return new Vector3(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z);
		}

		public static Vector3 operator *(Vector3 lhs, float rhs)
		{
			return new Vector3(rhs * lhs.X, rhs * lhs.Y, rhs * lhs.Z);
		}

		public static Vector3 operator +(Vector3 lhs, Vector3 rhs)
		{
			return new Vector3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
		}

		public static Vector3 operator -(Vector3 lhs, Vector3 rhs)
		{
			return new Vector3(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
		}

		public static Vector3 operator -(Vector3 value)
		{
			return new Vector3(-value.X, -value.Y, -value.Z);
		}

		public static float DotProduct(Vector3 value1, Vector3 value2)
		{
			return value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
		}

		public static Vector3 CrossProduct(Vector3 value1, Vector3 value2)
		{
			return new Vector3(
				value1.Y * value2.Z - value1.Z * value2.Y,
				value1.Z * value2.X - value1.X * value2.Z,
				value1.X * value2.Y - value1.Y * value2.X
			);
		}

		/// <summary>
		/// Returns this <see cref="Vector3"/> as a unit vector with the same direction.
		/// </summary>
		public Vector3 Normalized
		{
			get
			{
				var v = new Vector3(X, Y, Z);
				var length = Length;
				if (length > 0)
				{
					v.X /= length;
					v.Y /= length;
					v.Z /= length;
				}
				return v;
			}
		}

		public float Length
		{
			get { return Mathf.Sqrt(X * X + Y * Y + Z * Z); }
		}

		public float SqrLength
		{
			get { return X * X + Y * Y + Z * Z; }
		}

		/// <summary>
		/// Converts the string representation of the number to its <see cref="Vector3"/> equivalent.
		/// </summary>
		/// <param name="s">The string containing the vector to convert.</param>
		/// <example>"12, 34, 56".</example>
		public static Vector3 Parse(string s)
		{
			if (s == null) {
				throw new ArgumentNullException();
			}
			Vector3 vector;
			if (!TryParse(s, out vector)) {
				throw new FormatException();
			}
			return vector;
		}

		/// <summary>
		/// Converts the string representation of the vector to its <see cref="Vector3"/> equivalent.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="s">The string containing the vector to convert.</param>
		/// <example>"12, 34, 56".</example>
		/// <param name="vector">The result of conversion if it succeeds, <see cref="Zero"/> otherwise.</param>
		public static bool TryParse(string s, out Vector3 vector)
		{
			vector = Zero;
			if (s.IsNullOrWhiteSpace()) {
				return false;
			}

			var parts = s.Split(new [] {", "}, StringSplitOptions.None);
			if (parts.Length != 3 || parts.Any(i => i.IsNullOrWhiteSpace())) {
				return false;
			}

			return float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.X)
				&& float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.Y)
				&& float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.Z);
		}

		/// <summary>
		/// Returns the <see cref="string"/> representation of this <see cref="Vector3"/> in the format:
		/// "X, Y, Z".
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", X, Y, Z);
		}

		/// <summary>
		/// Returns the <see cref="string"/> representation of this <see cref="Vector3"/> in the format:
		/// "X, Y, Z".
		/// </summary>
		public string ToString(IFormatProvider format)
		{
			return string.Format(format, "{0}, {1}, {2}", X, Y, Z);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				return hashCode;
			}
		}
	}
}
