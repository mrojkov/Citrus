using System;
using System.Globalization;
using System.Linq;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Representation of 2D vectors and points.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[YuzuCompact]
	public struct Vector2 : IEquatable<Vector2>
	{
		[YuzuMember("0")]
		public float X;

		[YuzuMember("1")]
		public float Y;

		/// <summary>
		/// Returns a vector with components 0, 0.
		/// </summary>
		public static readonly Vector2 Zero = new Vector2(0, 0);

		/// <summary>
		/// Returns a vector with components 1, 1.
		/// </summary>
		public static readonly Vector2 One = new Vector2(1, 1);

		/// <summary>
		/// Returns a vector with components 0.5, 0.5.
		/// </summary>
		public static readonly Vector2 Half = new Vector2(0.5f, 0.5f);

		/// <summary>
		/// Returns a vector with components 0, -1.
		/// </summary>
		public static readonly Vector2 North = new Vector2(0, -1);

		/// <summary>
		/// Returns a vector with components 0, 1.
		/// </summary>
		public static readonly Vector2 South = new Vector2(0, 1);

		/// <summary>
		/// Returns a vector with components 1, 0.
		/// </summary>
		public static readonly Vector2 East = new Vector2(1, 0);

		/// <summary>
		/// Returns a vector with components -1, 0.
		/// </summary>
		public static readonly Vector2 West = new Vector2(-1, 0);

		/// <summary>
		/// Returns a vector with components Infinity, Infinity.
		/// </summary>
		public static readonly Vector2 PositiveInfinity = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

		/// <summary>
		/// Returns a vector with components -Infinity, -Infinity.
		/// </summary>
		public static readonly Vector2 NegativeInfinity = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

		/// <summary>
		/// Returns a vector with components 0, -1.
		/// </summary>
		public static readonly Vector2 Up = new Vector2(0, -1);

		/// <summary>
		/// Returns a vector with components 0, 1.
		/// </summary>
		public static readonly Vector2 Down = new Vector2(0, 1);

		/// <summary>
		/// Returns a vector with components -1, 0.
		/// </summary>
		public static readonly Vector2 Left = new Vector2(-1, 0);

		/// <summary>
		/// Returns a vector with components 1, 0.
		/// </summary>
		public static readonly Vector2 Right = new Vector2(1, 0);

		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public Vector2(float xy)
		{
			X = xy;
			Y = xy;
		}

		public static explicit operator IntVector2(Vector2 value)
		{
			return new IntVector2((int)value.X, (int)value.Y);
		}

		public static explicit operator Vector3(Vector2 value)
		{
			return new Vector3(value.X, value.Y, 0);
		}

		public static explicit operator Size(Vector2 value)
		{
			return new Size((int)value.X, (int)value.Y);
		}

		public bool Equals(Vector2 other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			return obj is Vector2 && Equals((Vector2) obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				return (X.GetHashCode() * 397) ^ Y.GetHashCode();
			}
		}

		public static bool operator == (Vector2 lhs, Vector2 rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		public static bool operator != (Vector2 lhs, Vector2 rhs)
		{
			return lhs.X != rhs.X || lhs.Y != rhs.Y;
		}

		/// <summary>
		/// Gets the angle (in degrees) between two <see cref="Vector2"/> instances.
		/// </summary>
		public static float AngleDeg(Vector2 value1, Vector2 value2)
		{
			return AngleRad(value1, value2) * Mathf.RadToDeg;
		}

		/// <summary>
		/// Gets the angle (in radians) between two <see cref="Vector2"/> instances.
		/// </summary>
		public static float AngleRad(Vector2 value1, Vector2 value2)
		{
			var sin = value1.X * value2.Y - value2.X * value1.Y;
			var cos = value1.X * value2.X + value1.Y * value2.Y;
			return Mathf.Atan2(sin, cos);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains
		/// linear interpolation of the specified vectors.
		/// </summary>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		public static Vector2 Lerp(float amount, Vector2 value1, Vector2 value2)
		{
			return new Vector2
			{
				X = Mathf.Lerp(amount, value1.X, value2.X),
				Y = Mathf.Lerp(amount, value1.Y, value2.Y)
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
				} else {
					throw new IndexOutOfRangeException();
				}
			}
		}

		/// <summary>
		/// Returns the distance between two vectors.
		/// </summary>
		public static float Distance(Vector2 value1, Vector2 value2)
		{
			return (value1 - value2).Length;
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

		public static Vector2 operator -(Vector2 value)
		{
			return new Vector2(-value.X, -value.Y);
		}

		public static float DotProduct(Vector2 value1, Vector2 value2)
		{
			return value1.X * value2.X + value1.Y * value2.Y;
		}

		public static float CrossProduct(Vector2 value1, Vector2 value2)
		{
			return value1.X * value2.Y - value1.Y * value2.X;
		}

		public static Vector2 Min(Vector2 a, Vector2 b)
		{
			return new Vector2(Mathf.Min(a.X, b.X), Mathf.Min(a.Y, b.Y));
		}

		public static Vector2 Max(Vector2 a, Vector2 b)
		{
			return new Vector2(Mathf.Max(a.X, b.X), Mathf.Max(a.Y, b.Y));
		}

		public static Vector2 Clamp(Vector2 value, Vector2 a, Vector2 b)
		{
			return new Vector2(Mathf.Clamp(value.X, a.X, b.X), Mathf.Clamp(value.Y, a.Y, b.Y));
		}

		public static Vector2 Round(Vector2 value)
		{
			return new Vector2(value.X.Round(), value.Y.Round());
		}

		public static Vector2 Ceiling(Vector2 value)
		{
			return new Vector2(value.X.Ceiling(), value.Y.Ceiling());
		}

		public static Vector2 Floor(Vector2 value)
		{
			return new Vector2(value.X.Floor(), value.Y.Floor());
		}

		public static Vector2 Truncate(Vector2 value)
		{
			return new Vector2(value.X.Truncate(), value.Y.Truncate());
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that represents
		/// cosine and sine of specified direction.
		/// </summary>
		/// <param name="radians">Azimuth of direction (in radians).</param>
		public static Vector2 CosSin(float radians)
		{
			return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that represents
		/// cosine and sine of specified direction.
		/// </summary>
		/// <param name="radians">Azimuth of direction (in radians).</param>
		public static Vector2 CosSinRough(float radians)
		{
			if (sinTable0 == null) {
				BuildSinTable();
			}
			const float t = 65536 / (2 * Mathf.Pi);
			int index = (int)(radians * t) & 65535;
			var a = sinTable0[index >> 8];
			var b = sinTable1[index & 255];
			Vector2 result;
			result.X = a.X * b.X - a.Y * b.Y;
			result.Y = a.Y * b.X + a.X * b.Y;
			return result;
		}

		/// <summary>
		/// Creates new <see cref="Vector2"/> that is turned around point (0, 0).
		/// </summary>
		/// <param name="value">Source vector.</param>
		/// <param name="degrees">Azimuth of turning (in degrees).</param>
		public static Vector2 RotateDeg(Vector2 value, float degrees)
		{
			return RotateRad(value, degrees * Mathf.DegToRad);
		}

		/// <summary>
		/// Creates new <see cref="Vector2"/> that is turned around point (0, 0).
		/// </summary>
		/// <param name="value">Source vector.</param>
		/// <param name="degrees">Azimuth of turning (in degrees).</param>
		public static Vector2 RotateDegRough(Vector2 value, float degrees)
		{
			return RotateRadRough(value, degrees * Mathf.DegToRad);
		}

		/// <summary>
		/// Creates new <see cref="Vector2"/> that is turned around point (0, 0).
		/// </summary>
		/// <param name="value">Source vector.</param>
		/// <param name="radians">Azimuth of turning (in radians).</param>
		public static Vector2 RotateRad(Vector2 value, float radians)
		{
			var cosSin = CosSin(radians);
			return new Vector2
			{
				X = value.X * cosSin.X - value.Y * cosSin.Y,
				Y = value.X * cosSin.Y + value.Y * cosSin.X
			};
		}

		/// <summary>
		/// Creates new <see cref="Vector2"/> that is turned around point (0, 0).
		/// </summary>
		/// <param name="value">Source vector.</param>
		/// <param name="radians">Azimuth of turning (in radians).</param>
		public static Vector2 RotateRadRough(Vector2 value, float radians)
		{
			var cosSin = CosSinRough(radians);
			return new Vector2
			{
				X = value.X * cosSin.X - value.Y * cosSin.Y,
				Y = value.X * cosSin.Y + value.Y * cosSin.X
			};
		}

		/// <summary>
		/// Returns the arctangent value of the current vector in the range of (-Pi, Pi].
		/// </summary>
		public float Atan2Rad
		{
			get { return Mathf.Atan2(Y, X); }
		}

		/// <summary>
		/// Returns the arctangent value of the current vector in the range of (-180, 180].
		/// </summary>
		public float Atan2Deg
		{
			get { return Mathf.Atan2(Y, X) * Mathf.RadToDeg; }
		}

		public float Length
		{
			get { return Mathf.Sqrt(X * X + Y * Y); }
		}

		/// <summary>
		/// Returns this <see cref="Vector2"/> as a unit vector with the same direction.
		/// </summary>
		public Vector2 Normalized
		{
			get
			{
				var v = new Vector2(X, Y);
				var length = Length;
				if (length > 0)
				{
					v.X /= length;
					v.Y /= length;
				}
				return v;
			}
		}

		public float SqrLength
		{
			get { return X * X + Y * Y; }
		}

		/// <summary>
		/// Returns the <see cref="string"/> representation of this <see cref="Vector2"/>
		/// in the format: "X, Y".
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0}, {1}", X, Y);
		}

		/// <summary>
		/// Returns the <see cref="string"/> representation of this <see cref="Vector2"/>
		/// in the format: "X, Y".
		/// </summary>
		public string ToString(IFormatProvider format)
		{
			return string.Format(format, "{0}, {1}", X, Y);
		}

		/// <summary>
		/// Converts the string representation of the vector to its <see cref="Vector2"/> equivalent.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="s">The string containing the vector to convert.</param>
		/// <example>"12, 34".</example>
		/// <param name="vector">The result of conversion if it succeeds, <see cref="Zero"/> otherwise.</param>
		public static bool TryParse(string s, out Vector2 vector)
		{
			vector = Zero;
			if (s.IsNullOrWhiteSpace()) {
				return false;
			}

			var parts = s.Split(new [] {", "}, StringSplitOptions.None);
			if (parts.Length != 2 || parts.Any(i => i.IsNullOrWhiteSpace())) {
				return false;
			}

			return float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.X)
				&& float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.Y);
		}

		/// <summary>
		/// Converts the string representation of the number to its <see cref="Vector2"/> equivalent.
		/// </summary>
		/// <param name="s">The string containing the vector to convert.</param>
		/// <example>"12, 34".</example>
		public static Vector2 Parse(string s)
		{
			if (s == null) {
				throw new ArgumentNullException();
			}
			Vector2 vector;
			if (!TryParse(s, out vector)) {
				throw new FormatException();
			}
			return vector;
		}

		private static Vector2[] sinTable0;
		private static Vector2[] sinTable1;

		/// <summary>
		/// Used for <see cref="CosSinRough(float)"/>.
		/// </summary>
		private static void BuildSinTable()
		{
			sinTable0 = new Vector2[256];
			sinTable1 = new Vector2[256];
			const float T1 = 2 * Mathf.Pi / 256;
			const float T2 = T1 / 256;
			for (int i = 0; i < 256; i++) {
				sinTable0[i] = new Vector2(Mathf.Cos(i * T1), Mathf.Sin(i * T1));
				sinTable1[i] = new Vector2(Mathf.Cos(i * T2), Mathf.Sin(i * T2));
			}
		}
	}
}
