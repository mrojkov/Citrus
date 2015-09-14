using System;
using System.Globalization;
using System.Linq;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Representation of 2D vectors and points.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct Vector2 : IEquatable<Vector2>
	{
		/// <summary>
		/// X component of this <see cref="Vector2"/>.
		/// </summary>
		[ProtoMember(1)]
		public float X;

		/// <summary>
		/// Y component of this <see cref="Vector2"/>.
		/// </summary>
		[ProtoMember(2)]
		public float Y;

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 0, 0.
		/// </summary>
		public static readonly Vector2 Zero = new Vector2(0, 0);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 1, 1.
		/// </summary>
		public static readonly Vector2 One = new Vector2(1, 1);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 0.5, 0.5.
		/// </summary>
		public static readonly Vector2 Half = new Vector2(0.5f, 0.5f);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 0, -1.
		/// </summary>
		public static readonly Vector2 North = new Vector2(0, -1);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 0, 1.
		/// </summary>
		public static readonly Vector2 South = new Vector2(0, 1);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 1, 0.
		/// </summary>
		public static readonly Vector2 East = new Vector2(1, 0);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components -1, 0.
		/// </summary>
		public static readonly Vector2 West = new Vector2(-1, 0);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components Infinity, Infinity.
		/// </summary>
		public static readonly Vector2 PositiveInfinity = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components -Infinity, -Infinity.
		/// </summary>
		public static readonly Vector2 NegativeInfinity = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 0, -1.
		/// </summary>
		public static readonly Vector2 Up = new Vector2(0, -1);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 0, 1.
		/// </summary>
		public static readonly Vector2 Down = new Vector2(0, 1);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components -1, 0.
		/// </summary>
		public static readonly Vector2 Left = new Vector2(-1, 0);

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 1, 0.
		/// </summary>
		public static readonly Vector2 Right = new Vector2(1, 0);

		/// <summary>
		/// Constructs a 2D vector with X and Y from two values.
		/// </summary>
		/// <param name="x">The X coordinate in 2D-space.</param>
		/// <param name="y">The Y coordinate in 2D-space.</param>
		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Constructs a 2D vector with X and Y set to the same value.
		/// </summary>
		/// <param name="value">The X and Y coordinates in 2d-space.</param>
		public Vector2(float value)
		{
			X = value;
			Y = value;
		}

		/// <summary>
		/// Explicit cast from <see cref="Vector2"/> to <see cref="IntVector2"/>.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/>.</param>
		/// <returns>
		/// New <see cref="IntVector2"/> with truncated X and Y of source <see cref="Vector2"/>.
		/// </returns>
		public static explicit operator IntVector2(Vector2 value)
		{
			return new IntVector2((int)value.X, (int)value.Y);
		}

		/// <summary>
		/// Explicit cast from <see cref="Vector2"/> to <see cref="Vector3"/>.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/>.</param>
		/// <returns>
		/// New <see cref="Vector3"/> with X and Y of source <see cref="Vector2"/> and Z equals 0.
		/// </returns>
		public static explicit operator Vector3(Vector2 value)
		{
			return new Vector3(value.X, value.Y, 0);
		}

		/// <summary>
		/// Explicit cast from <see cref="Vector2"/> to <see cref="Size"/>.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/>.</param>
		/// <returns>New <see cref="Size"/> with X and Y of source <see cref="Vector2"/>.</returns>
		public static explicit operator Size(Vector2 value)
		{
			return new Size((int)value.X, (int)value.Y);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Vector2"/>.
		/// </summary>
		/// <param name="other">The <see cref="Vector2"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// Compairing is done without taking floating point error into account.
		/// </remarks>
		public bool Equals(Vector2 other)
		{
			return X == other.X && Y == other.Y;
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// Compairing is done without taking floating point error into account.
		/// </remarks>
		public override bool Equals(object obj)
		{
			return obj is Vector2 && Equals((Vector2) obj);
		}

		/// <summary>
		/// Gets the hash code of this <see cref="Vector2"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Vector2"/>.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		/// <summary>
		/// Compares whether two <see cref="Vector2"/> instances are equal.
		/// </summary>
		/// <param name="lhs"><see cref="Vector2"/> instance on the left of the equal sign.</param>
		/// <param name="rhs"><see cref="Vector2"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// Compairing is done without taking floating point error into account.
		/// </remarks>
		public static bool operator == (Vector2 lhs, Vector2 rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		/// <summary>
		/// Compares whether two <see cref="Vector2"/> instances are not equal.
		/// </summary>
		/// <param name="lhs"><see cref="Vector2"/> instance on the left of the not equal sign.</param>
		/// <param name="rhs"><see cref="Vector2"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
		/// <remarks>
		/// Compairing is done without taking floating point error into account.
		/// </remarks>
		public static bool operator != (Vector2 lhs, Vector2 rhs)
		{
			return lhs.X != rhs.X || lhs.Y != rhs.Y;
		}

		/// <summary>
		/// Gets the angle (in degrees) between two <see cref="Vector2"/> instances.
		/// </summary>
		/// <param name="value1">The first <see cref="Vector2"/>.</param>
		/// <param name="value2">The second <see cref="Vector2"/>.</param>
		/// <returns>The angle between two <see cref="Vector2"/> in degrees.</returns>	
		public static float AngleDeg(Vector2 value1, Vector2 value2)
		{
			return AngleRad(value1, value2) * Mathf.RadiansToDegrees;
		}

		/// <summary>
		/// Gets the angle (in radians) between two <see cref="Vector2"/> instances.
		/// </summary>
		/// <param name="value1">The first <see cref="Vector2"/>.</param>
		/// <param name="value2">The second <see cref="Vector2"/>.</param>
		/// <returns>The angle between two <see cref="Vector2"/> in radians.</returns>	
		public static float AngleRad(Vector2 value1, Vector2 value2)
		{
			var sin = value1.X * value2.Y - value2.X * value1.Y;
			var cos = value1.X * value2.X + value1.Y * value2.Y;
			return (float)Math.Atan2(sin, cos);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains 
		/// linear interpolation of the specified vectors.
		/// </summary>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="value1">The first <see cref="Vector2"/>.</param>
		/// <param name="value2">The second <see cref="Vector2"/>.</param>
		/// <returns>The result of linear interpolation of the specified vectors.</returns>
		public static Vector2 Lerp(float amount, Vector2 value1, Vector2 value2)
		{
			return new Vector2
			{
				X = Mathf.Lerp(amount, value1.X, value2.X),
				Y = Mathf.Lerp(amount, value1.Y, value2.Y)
			};
		}

		/// <summary>
		/// Returns the distance between two <see cref="Vector2"/> instances.
		/// </summary>
		/// <param name="value1">The first <see cref="Vector2"/>.</param>
		/// <param name="value2">The second <see cref="Vector2"/>.</param>
		/// <returns>The distance between two <see cref="Vector2"/>.</returns>
		public static float Distance(Vector2 value1, Vector2 value2)
		{
			return (value1 - value2).Length;
		}

		/// <summary>
		/// Multiplies vectors componentwise.
		/// </summary>
		/// <param name="lhs">Source <see cref="Vector2"/> on the left of the mul sign.</param>
		/// <param name="rhs">Source <see cref="Vector2"/> on the right of the mul sign.</param>
		/// <returns>The result of the <see cref="Vector2"/> multiplication.</returns>
		public static Vector2 operator *(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X * rhs.X, lhs.Y * rhs.Y);
		}

		/// <summary>
		/// Divides vectors componentwise.
		/// </summary>
		/// <param name="lhs">Source <see cref="Vector2"/> on the left of the div sign.</param>
		/// <param name="rhs">Divisor <see cref="Vector2"/> on the right of the div sign.</param>
		/// <returns>The result of the <see cref="Vector2"/> divide.</returns>
		public static Vector2 operator /(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X / rhs.X, lhs.Y / rhs.Y);
		}

		/// <summary>
		/// Divides the components of the <see cref="Vector2"/> by a scalar.
		/// </summary>
		/// <param name="lhs">Source <see cref="Vector2"/> on the left of the div sign.</param>
		/// <param name="rhs">Divisor scalar on the right of the div sign.</param>
		/// <returns>The result of dividing the <see cref="Vector2"/> by a scalar.</returns>
		public static Vector2 operator /(Vector2 lhs, float rhs)
		{
			return new Vector2(lhs.X / rhs, lhs.Y / rhs);
		}

		/// <summary>
		/// Multiplies the components of <see cref="Vector2"/> by a scalar.
		/// </summary>
		/// <param name="lhs">Scalar value on the left of the mul sign.</param>
		/// <param name="rhs">Source <see cref="Vector2"/> on the right of the mul sign.</param>
		/// <returns>The result of the <see cref="Vector2"/> multiplication with a scalar.</returns>
		public static Vector2 operator *(float lhs, Vector2 rhs)
		{
			return new Vector2(lhs * rhs.X, lhs * rhs.Y);
		}

		/// <summary>
		/// Multiplies the components of <see cref="Vector2"/> by a scalar.
		/// </summary>
		/// <param name="lhs">Source <see cref="Vector2"/> on the left of the mul sign.</param>
		/// <param name="rhs">Scalar value on the right of the mul sign.</param>
		/// <returns>The result of the <see cref="Vector2"/> multiplication with a scalar.</returns>
		public static Vector2 operator *(Vector2 lhs, float rhs)
		{
			return new Vector2(rhs * lhs.X, rhs * lhs.Y);
		}

		/// <summary>
		/// Adds vectors componentwise.
		/// </summary>
		/// <param name="lhs">Source <see cref="Vector2"/> on the left of the add sign.</param>
		/// <param name="rhs">Source <see cref="Vector2"/> on the right of the add sign.</param>
		/// <returns>The result of the <see cref="Vector2"/> sum.</returns>
		public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
		}

		/// <summary>
		/// Subtracts vectors componentwise.
		/// </summary>
		/// <param name="lhs">Source <see cref="Vector2"/> on the left of the sub sign.</param>
		/// <param name="rhs">Divisor <see cref="Vector2"/> on the right of the sub sign.</param>
		/// <returns>The result of the <see cref="Vector2"/> subtract.</returns>
		public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
		}

		/// <summary>
		/// Inverts values in the specified <see cref="Vector2"/>.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/> on the right of the sub sign.</param>
		/// <returns>Result of the inversion.</returns>
		public static Vector2 operator -(Vector2 value)
		{
			return new Vector2(-value.X, -value.Y);
		}

		/// <summary>
		/// Returns a dot product of two <see cref="Vector2"/> instances.
		/// </summary>
		/// <param name="value1">The first <see cref="Vector2"/>.</param>
		/// <param name="value2">The second <see cref="Vector2"/>.</param>
		/// <returns>The dot product of two <see cref="Vector2"/> instances.</returns>
		public static float DotProduct(Vector2 value1, Vector2 value2)
		{
			return value1.X * value2.X + value1.Y * value2.Y;
		}

		/// <summary>
		/// Returns a cross product of two <see cref="Vector2"/> instances.
		/// </summary>
		/// <param name="value1">The first <see cref="Vector2"/>.</param>
		/// <param name="value2">The second <see cref="Vector2"/>.</param>
		/// <returns>The cross product of two <see cref="Vector2"/> instances.</returns>
		public static float CrossProduct(Vector2 value1, Vector2 value2)
		{
			return value1.X * value2.Y - value1.Y * value2.X;
		}

		/// <summary>
		/// Turns this <see cref="Vector2"/> to a unit vector with the same direction.
		/// </summary>
		public void Normalize()
		{
			var length = Length;
			if (length > 0) {
				X /= length;
				Y /= length;
			}
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that represents specified direction.
		/// </summary>
		/// <param name="degrees">Azimuth of direction (in degrees).</param>
		/// <returns>
		/// New <see cref="Vector2"/> that represents specified direction (in degrees).
		/// </returns>
		public static Vector2 HeadingDeg(float degrees)
		{
			return Mathf.CosSin(degrees * Mathf.DegreesToRadians);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that represents specified direction.
		/// </summary>
		/// <param name="radians">Azimuth of direction (in radians).</param>
		/// <returns>
		/// New <see cref="Vector2"/> that represents specified direction (in radians).
		/// </returns>
		public static Vector2 HeadingRad(float radians)
		{
			return Mathf.CosSin(radians);
		}

		/// <summary>
		/// Creates new <see cref="Vector2"/> that is turned around point (0, 0).
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/>.</param>
		/// <param name="degrees">Azimuth of turning (in degrees).</param>
		/// <returns>
		/// New <see cref="Vector2"/> that represents source <see cref="Vector2"/> 
		/// turned by specified angle (in degrees).
		/// </returns>
		public static Vector2 RotateDeg(Vector2 value, float degrees)
		{
			return RotateRad(value, degrees * Mathf.DegreesToRadians);
		}

		/// <summary>
		/// Creates new <see cref="Vector2"/> that is turned around point (0, 0).
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/>.</param>
		/// <param name="radians">Azimuth of turning (in radians).</param>
		/// <returns>
		/// New <see cref="Vector2"/> that represents source <see cref="Vector2"/> 
		/// turned by specified angle (in radians).
		/// </returns>
		public static Vector2 RotateRad(Vector2 value, float radians)
		{
			var cosSin = Mathf.CosSin(radians);
			return new Vector2
			{
				X = value.X * cosSin.X - value.Y * cosSin.Y,
				Y = value.X * cosSin.Y + value.Y * cosSin.X
			};
		}

		/// <summary>
		/// Returns the arctangent value of the current vector in the range of (-Pi, Pi].
		/// </summary>
		/// <returns>The arctangent value of the current vector in the range of (-Pi, Pi].</returns>
		public float Atan2Rad
		{
			get { return (float)Math.Atan2(Y, X); }
		}

		/// <summary>
		/// Returns the arctangent value of the current vector in the range of (-180, 180].
		/// </summary>
		/// <returns>The arctangent value of the current vector in the range of (-180, 180].</returns>
		public float Atan2Deg
		{
			get { return (float)Math.Atan2(Y, X) * Mathf.RadiansToDegrees; }
		}

		/// <summary>
		/// Returns the length of this <see cref="Vector2"/>.
		/// </summary>
		/// <returns>The length of this <see cref="Vector2"/>.</returns>
		public float Length
		{
			get { return (float)Math.Sqrt(X * X + Y * Y); }
		}

		/// <summary>
		/// Returns this <see cref="Vector2"/> as a unit vector with the same direction.
		/// </summary>
		public Vector2 Normalized
		{
			get 
			{
				var v = new Vector2(X, Y);
				v.Normalize();
				return v;
			}
		}

		/// <summary>
		/// Returns the squared length of this <see cref="Vector2"/>.
		/// </summary>
		/// <returns>The squared length of this <see cref="Vector2"/>.</returns>
		public float SqrLength
		{
			get { return X * X + Y * Y; }
		}

		/// <summary>
		/// Returns the <see cref="String"/> representation of this <see cref="Vector2"/> in the format:
		/// "<see cref="X"/>, <see cref="Y"/>".
		/// </summary>
		/// <returns>The <see cref="String"/> representation of this <see cref="Vector2"/>.</returns>
		public override string ToString()
		{
			return string.Format("{0}, {1}", X, Y);
		}

		/// <summary>
		/// Converts the string representation of the vector to its <see cref="Vector2.Zero"/> equivalent.
		/// The return value indicates whether the conversion succeeded.
		/// </summary>
		/// <param name="s">The string containing the <see cref="Vector2"/> to convert.</param>
		/// <example>"12, 34".</example>
		/// <param name="vector">The result of conversion if it succeeds,
		/// <see cref="Vector2.Zero"/> otherwise.</param>
		/// <returns><c>true</c> if operation succeeds; <c>false</c> otherwise.</returns>	
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
		/// Converts the string representation of the number to its <see cref="Vector2.Zero"/> equivalent.
		/// </summary>
		/// <param name="s">The string containing the <see cref="Vector2"/> to convert.</param>
		/// <example>"12, 34".</example>
		/// <returns>The <see cref="Vector2"/> equivalent to the vector contained in s.</returns>
		/// <exception cref="ArgumentNullException">s is null.</exception>
		/// <exception cref="FormatException">s is not in the correct format.</exception>
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
	}
}