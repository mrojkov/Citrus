using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Lime;
using Yuzu;

namespace Tangerine.UI.SceneView.WidgetTransforms
{
	/// <summary>
	/// Representation of 2D vectors and points.
	/// </summary>
	[DebuggerStepThrough]
	[YuzuCompact]
	public struct Vector2d : IEquatable<Vector2d>
	{
		[YuzuMember("0")]
		public double X;

		[YuzuMember("1")]
		public double Y;

		/// <summary>
		/// Returns a vector with components 0, 0.
		/// </summary>
		public static readonly Vector2d Zero = new Vector2d(0, 0);

		/// <summary>
		/// Returns a vector with components 1, 1.
		/// </summary>
		public static readonly Vector2d One = new Vector2d(1, 1);

		/// <summary>
		/// Returns a vector with components 0.5, 0.5.
		/// </summary>
		public static readonly Vector2d Half = new Vector2d(0.5, 0.5);

		/// <summary>
		/// Returns a vector with components 0, -1.
		/// </summary>
		public static readonly Vector2d North = new Vector2d(0, -1);

		/// <summary>
		/// Returns a vector with components 0, 1.
		/// </summary>
		public static readonly Vector2d South = new Vector2d(0, 1);

		/// <summary>
		/// Returns a vector with components 1, 0.
		/// </summary>
		public static readonly Vector2d East = new Vector2d(1, 0);

		/// <summary>
		/// Returns a vector with components -1, 0.
		/// </summary>
		public static readonly Vector2d West = new Vector2d(-1, 0);

		/// <summary>
		/// Returns a vector with components Infinity, Infinity.
		/// </summary>
		public static readonly Vector2d PositiveInfinity = new Vector2d(double.PositiveInfinity, double.PositiveInfinity);

		/// <summary>
		/// Returns a vector with components -Infinity, -Infinity.
		/// </summary>
		public static readonly Vector2d NegativeInfinity = new Vector2d(double.NegativeInfinity, double.NegativeInfinity);

		/// <summary>
		/// Returns a vector with components 0, -1.
		/// </summary>
		public static readonly Vector2d Up = new Vector2d(0, -1);

		/// <summary>
		/// Returns a vector with components 0, 1.
		/// </summary>
		public static readonly Vector2d Down = new Vector2d(0, 1);

		/// <summary>
		/// Returns a vector with components -1, 0.
		/// </summary>
		public static readonly Vector2d Left = new Vector2d(-1, 0);

		/// <summary>
		/// Returns a vector with components 1, 0.
		/// </summary>
		public static readonly Vector2d Right = new Vector2d(1, 0);

		public Vector2d(double x, double y)
		{
			X = x;
			Y = y;
		}

		public Vector2d(double xy)
		{
			X = xy;
			Y = xy;
		}

		public static explicit operator IntVector2(Vector2d value)
		{
			return new IntVector2((int)value.X, (int)value.Y);
		}

		public static explicit operator Vector2(Vector2d value)
		{
			return new Vector2((float)value.X, (float)value.Y);
		}

		public static explicit operator Vector2d(Vector2 value)
		{
			return new Vector2d(value.X, value.Y);
		}

		public static explicit operator Size(Vector2d value)
		{
			return new Size((int)value.X, (int)value.Y);
		}

		public bool Equals(Vector2d other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			return obj is Vector2d && Equals((Vector2d) obj);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public static bool operator == (Vector2d lhs, Vector2d rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		public static bool operator != (Vector2d lhs, Vector2d rhs)
		{
			return lhs.X != rhs.X || lhs.Y != rhs.Y;
		}

		/// <summary>
		/// Gets the angle (in degrees) between two <see cref="Vector2d"/> instances.
		/// </summary>
		public static double AngleDeg(Vector2d value1, Vector2d value2)
		{
			return AngleRad(value1, value2) * 180.0 / Math.PI;
		}

		/// <summary>
		/// Gets the angle (in radians) between two <see cref="Vector2d"/> instances.
		/// </summary>
		public static double AngleRad(Vector2d value1, Vector2d value2)
		{
			var sin = value1.X * value2.Y - value2.X * value1.Y;
			var cos = value1.X * value2.X + value1.Y * value2.Y;
			return Math.Atan2(sin, cos);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2d"/> that contains
		/// linear interpolation of the specified vectors.
		/// </summary>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		public static Vector2d Lerp(double amount, Vector2d value1, Vector2d value2)
		{
			return new Vector2d
			{
				X = Mathd.Lerp(amount, value1.X, value2.X),
				Y = Mathd.Lerp(amount, value1.Y, value2.Y)
			};
		}

		/// <summary>
		/// Gets or sets the vector component by its index.
		/// </summary>
		public double this [int component]
		{
			get {
				if (component == 0) {
					return X;
				}
				if (component == 1) {
					return Y;
				}
				throw new IndexOutOfRangeException();
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
		public static double Distance(Vector2d value1, Vector2d value2)
		{
			return (value1 - value2).Length;
		}

		public static Vector2d operator *(Vector2d lhs, Vector2d rhs)
		{
			return new Vector2d(lhs.X * rhs.X, lhs.Y * rhs.Y);
		}

		public static Vector2d operator /(Vector2d lhs, Vector2d rhs)
		{
			return new Vector2d(lhs.X / rhs.X, lhs.Y / rhs.Y);
		}

		public static Vector2d operator /(Vector2d lhs, double rhs)
		{
			return new Vector2d(lhs.X / rhs, lhs.Y / rhs);
		}

		public static Vector2d operator *(double lhs, Vector2d rhs)
		{
			return new Vector2d(lhs * rhs.X, lhs * rhs.Y);
		}

		public static Vector2d operator *(Vector2d lhs, double rhs)
		{
			return new Vector2d(rhs * lhs.X, rhs * lhs.Y);
		}

		public static Vector2d operator +(Vector2d lhs, Vector2d rhs)
		{
			return new Vector2d(lhs.X + rhs.X, lhs.Y + rhs.Y);
		}

		public static Vector2d operator -(Vector2d lhs, Vector2d rhs)
		{
			return new Vector2d(lhs.X - rhs.X, lhs.Y - rhs.Y);
		}

		public static Vector2d operator -(Vector2d value)
		{
			return new Vector2d(-value.X, -value.Y);
		}

		public static double DotProduct(Vector2d value1, Vector2d value2)
		{
			return value1.X * value2.X + value1.Y * value2.Y;
		}

		public static double CrossProduct(Vector2d value1, Vector2d value2)
		{
			return value1.X * value2.Y - value1.Y * value2.X;
		}

		public static Vector2d Min(Vector2d a, Vector2d b)
		{
			return new Vector2d(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
		}

		public static Vector2d Max(Vector2d a, Vector2d b)
		{
			return new Vector2d(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
		}

		public static Vector2d Round(Vector2d value)
		{
			return new Vector2d(value.X.Round(), value.Y.Round());
		}

		/// <summary>
		/// Creates a new <see cref="Vector2d"/> that represents
		/// cosine and sine of specified direction.
		/// </summary>
		/// <param name="radians">Azimuth of direction (in radians).</param>
		public static Vector2d CosSin(double radians)
		{
			return new Vector2d(Math.Cos(radians), Math.Sin(radians));
		}

		/// <summary>
		/// Creates new <see cref="Vector2d"/> that is turned around point (0, 0).
		/// </summary>
		/// <param name="value">Source vector.</param>
		/// <param name="degrees">Azimuth of turning (in degrees).</param>
		public static Vector2d RotateDeg(Vector2d value, double degrees)
		{
			return RotateRad(value, degrees * Math.PI / 180.0);
		}

		/// <summary>
		/// Creates new <see cref="Vector2d"/> that is turned around point (0, 0).
		/// </summary>
		/// <param name="value">Source vector.</param>
		/// <param name="radians">Azimuth of turning (in radians).</param>
		public static Vector2d RotateRad(Vector2d value, double radians)
		{
			var cosSin = CosSin(radians);
			return new Vector2d
			{
				X = value.X * cosSin.X - value.Y * cosSin.Y,
				Y = value.X * cosSin.Y + value.Y * cosSin.X
			};
		}

		/// <summary>
		/// Returns the arctangent value of the current vector in the range of (-Pi, Pi].
		/// </summary>
		public double Atan2Rad
		{
			get { return Math.Atan2(Y, X); }
		}

		/// <summary>
		/// Returns the arctangent value of the current vector in the range of (-180, 180].
		/// </summary>
		public double Atan2Deg
		{
			get { return Math.Atan2(Y, X) * 180.0 / Math.PI; }
		}

		public double Length
		{
			get { return Math.Sqrt(X * X + Y * Y); }
		}

		/// <summary>
		/// Returns this <see cref="Vector2d"/> as a unit vector with the same direction.
		/// </summary>
		public Vector2d Normalized
		{
			get
			{
				var v = new Vector2d(X, Y);
				var length = Length;
				if (length > 0)
				{
					v.X /= length;
					v.Y /= length;
				}
				return v;
			}
		}

		public double SqrLength
		{
			get { return X * X + Y * Y; }
		}

		/// <summary>
		/// Returns the <see cref="string"/> representation of this <see cref="Vector2d"/>
		/// in the format: "X, Y".
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0}, {1}", X, Y);
		}

		/// <summary>
		/// Returns the <see cref="string"/> representation of this <see cref="Vector2d"/>
		/// in the format: "X, Y".
		/// </summary>
		public string ToString(IFormatProvider format)
		{
			return string.Format(format, "{0}, {1}", X, Y);
		}

		/// <summary>
		/// Converts the string representation of the vector to its <see cref="Vector2d"/> equivalent.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="s">The string containing the vector to convert.</param>
		/// <example>"12, 34".</example>
		/// <param name="vector">The result of conversion if it succeeds, <see cref="Zero"/> otherwise.</param>
		public static bool TryParse(string s, out Vector2d vector)
		{
			vector = Zero;
			if (s.IsNullOrWhiteSpace()) {
				return false;
			}

			var parts = s.Split(new [] {", "}, StringSplitOptions.None);
			if (parts.Length != 2 || parts.Any(i => i.IsNullOrWhiteSpace())) {
				return false;
			}

			return double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.X)
				&& double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.Y);
		}

		/// <summary>
		/// Converts the string representation of the number to its <see cref="Vector2d"/> equivalent.
		/// </summary>
		/// <param name="s">The string containing the vector to convert.</param>
		/// <example>"12, 34".</example>
		public static Vector2d Parse(string s)
		{
			if (s == null) {
				throw new ArgumentNullException();
			}
			Vector2d vector;
			if (!TryParse(s, out vector)) {
				throw new FormatException();
			}
			return vector;
		}

	}
}
