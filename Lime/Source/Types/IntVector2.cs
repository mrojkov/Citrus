using System;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Representation of integer 2D vectors and points.
	/// </summary>
	[ProtoContract]
	[System.Diagnostics.DebuggerStepThrough]
	public struct IntVector2 : IEquatable<IntVector2>
	{
		/// <summary>
		/// X component of this <see cref="IntVector2"/>.
		/// </summary>
		[ProtoMember(1)]
		public int X;

		/// <summary>
		/// Y component of this <see cref="IntVector2"/>.
		/// </summary>
		[ProtoMember(2)]
		public int Y;

		/// <summary>
		/// Returns a <see cref="IntVector2"/> with components 0, 0.
		/// </summary>
		public static readonly IntVector2 Zero = new IntVector2(0, 0);

		/// <summary>
		/// Returns a <see cref="IntVector2"/> with components 1, 1.
		/// </summary>
		public static readonly IntVector2 One = new IntVector2(1, 1);

		/// <summary>
		/// Returns a <see cref="IntVector2"/> with components 0, -1.
		/// </summary>
		public static readonly IntVector2 Up = new IntVector2(0, -1);

		/// <summary>
		/// Returns a <see cref="IntVector2"/> with components 0, 1.
		/// </summary>
		public static readonly IntVector2 Down = new IntVector2(0, 1);

		/// <summary>
		/// Returns a <see cref="IntVector2"/> with components -1, 0.
		/// </summary>
		public static readonly IntVector2 Left = new IntVector2(-1, 0);

		/// <summary>
		/// Returns a <see cref="IntVector2"/> with components 1, 0.
		/// </summary>
		public static readonly IntVector2 Right = new IntVector2(1, 0);

		/// <summary>
		/// Constructs a 2D vector with X and Y from two values.
		/// </summary>
		/// <param name="x">The X coordinate in 2D-space.</param>
		/// <param name="y">The Y coordinate in 2D-space.</param>
		public IntVector2(int x, int y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Explicit cast from <see cref="IntVector2"/> to <see cref="Size"/>.
		/// </summary>
		/// <param name="value">Source <see cref="IntVector2"/>.</param>
		/// <returns>
		/// New <see cref="Size"/> with X and Y of source <see cref="IntVector2"/>.
		/// </returns>
		public static explicit operator Size(IntVector2 value)
		{
			return new Size(value.X, value.Y);
		}

		/// <summary>
		/// Explicit cast from <see cref="IntVector2"/> to <see cref="Vector2"/>.
		/// </summary>
		/// <param name="value">Source <see cref="IntVector2"/>.</param>
		/// <returns>
		/// New <see cref="Vector2"/> with X and Y of source <see cref="IntVector2"/>.
		/// </returns>
		public static explicit operator Vector2(IntVector2 value)
		{
			return new Vector2(value.X, value.Y);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="IntVector2"/>.
		/// </summary>
		/// <param name="other">The <see cref="IntVector2"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(IntVector2 other)
		{
			return X == other.X && Y == other.Y;
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			return obj is IntVector2 && Equals((IntVector2)obj);
		}

		/// <summary>
		/// Gets the hash code of this <see cref="IntVector2"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="IntVector2"/>.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		/// <summary>
		/// Compares whether two <see cref="IntVector2"/> instances are equal.
		/// </summary>
		/// <param name="lhs"><see cref="IntVector2"/> instance on the left of the equal sign.</param>
		/// <param name="rhs"><see cref="IntVector2"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(IntVector2 lhs, IntVector2 rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		/// <summary>
		/// Compares whether two <see cref="IntVector2"/> instances are not equal.
		/// </summary>
		/// <param name="lhs"><see cref="IntVector2"/> instance on the left of the not equal sign.</param>
		/// <param name="rhs"><see cref="IntVector2"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
		public static bool operator !=(IntVector2 lhs, IntVector2 rhs)
		{
			return lhs.X != rhs.X || lhs.Y != rhs.Y;
		}

		/// <summary>
		/// Adds vectors componentwise.
		/// </summary>
		/// <param name="lhs">Source <see cref="IntVector2"/> on the left of the add sign.</param>
		/// <param name="rhs">Source <see cref="IntVector2"/> on the right of the add sign.</param>
		/// <returns>The result of the <see cref="IntVector2"/> sum.</returns>
		public static IntVector2 operator +(IntVector2 lhs, IntVector2 rhs)
		{
			return new IntVector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
		}

		/// <summary>
		/// Subtracts vectors componentwise.
		/// </summary>
		/// <param name="lhs">Source <see cref="IntVector2"/> on the left of the sub sign.</param>
		/// <param name="rhs">Divisor <see cref="IntVector2"/> on the right of the sub sign.</param>
		/// <returns>The result of the <see cref="IntVector2"/> subtract.</returns>
		public static IntVector2 operator -(IntVector2 lhs, IntVector2 rhs)
		{
			return new IntVector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
		}

		/// <summary>
		/// Inverts values in the specified <see cref="IntVector2"/>.
		/// </summary>
		/// <param name="value">Source <see cref="IntVector2"/> on the right of the sub sign.</param>
		/// <returns>Result of the inversion.</returns>
		public static IntVector2 operator -(IntVector2 value)
		{
			return new IntVector2(-value.X, -value.Y);
		}

		/// <summary>
		/// Divides the components of the <see cref="IntVector2"/> by a scalar.
		/// </summary>
		/// <param name="lhs">Source <see cref="IntVector2"/> on the left of the div sign.</param>
		/// <param name="rhs">Divisor scalar on the right of the div sign.</param>
		/// <returns>The result of dividing the <see cref="IntVector2"/> by a scalar.</returns>
		public static IntVector2 operator /(IntVector2 lhs, int rhs)
		{
			return new IntVector2(lhs.X / rhs, lhs.Y / rhs);
		}

		/// <summary>
		/// Multiplies the components of <see cref="IntVector2"/> by a scalar.
		/// </summary>
		/// <param name="lhs">Scalar value on the left of the mul sign.</param>
		/// <param name="rhs">Source <see cref="IntVector2"/> on the right of the mul sign.</param>
		/// <returns>The result of the <see cref="IntVector2"/> multiplication with a scalar.</returns>
		public static IntVector2 operator *(int lhs, IntVector2 rhs)
		{
			return new IntVector2(lhs * rhs.X, lhs * rhs.Y);
		}

		/// <summary>
		/// Multiplies the components of <see cref="IntVector2"/> by a scalar.
		/// </summary>
		/// <param name="lhs">Source <see cref="IntVector2"/> on the left of the mul sign.</param>
		/// <param name="rhs">Scalar value on the right of the mul sign.</param>
		/// <returns>The result of the <see cref="IntVector2"/> multiplication with a scalar.</returns>
		public static IntVector2 operator *(IntVector2 lhs, int rhs)
		{
			return new IntVector2(rhs * lhs.X, rhs * lhs.Y);
		}

		/// <summary>
		/// Returns the <see cref="String"/> representation of this <see cref="IntVector2"/> in the format:
		/// "<see cref="X"/>, <see cref="Y"/>".
		/// </summary>
		/// <returns>The <see cref="String"/> representation of this <see cref="IntVector2"/>.</returns>
		public override string ToString()
		{
			return string.Format("{0}, {1}", X, Y);
		}

		/// <summary>
		/// Returns the <see cref="Vector2"/> representation of this <see cref="IntVector2"/>.
		/// </summary>
		/// <returns>The <see cref="Vector2"/> representation of this <see cref="IntVector2"/>.</returns>
		public Vector2 ToVector2()
		{
			return new Vector2(X, Y);
		}
	}
}