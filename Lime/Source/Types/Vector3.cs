using System;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Representation of 3D vectors and points.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct Vector3 : IEquatable<Vector3>
	{
		/// <summary>
		/// X component of this <see cref="Vector3"/>.
		/// </summary>
		[ProtoMember(1)]
		public float X;

		/// <summary>
		/// Y component of this <see cref="Vector3"/>.
		/// </summary>
		[ProtoMember(2)]
		public float Y;

		/// <summary>
		/// Z component of this <see cref="Vector3"/>.
		/// </summary>
		[ProtoMember(3)]
		public float Z;

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 0, 0, 0.
		/// </summary>
		public static readonly Vector3 Zero = new Vector3(0, 0, 0);

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 1, 1, 1.
		/// </summary>
		public static readonly Vector3 One = new Vector3(1, 1, 1);

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 0.5, 0.5, 0.5.
		/// </summary>
		public static readonly Vector3 Half = new Vector3(0.5f, 0.5f, 0.5f);

		/// <summary>
		/// Constructs a 3D vector with X, Y and Z from three values.
		/// </summary>
		/// <param name="x">The x coordinate in 3d-space.</param>
		/// <param name="y">The y coordinate in 3d-space.</param>
		/// <param name="z">The z coordinate in 3d-space.</param>
		public Vector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Explicit cast from <see cref="Vector3"/> to <see cref="Vector2"/>.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/>.</param>
		/// <returns>
		/// New <see cref="Vector2"/> with X and Y of source <see cref="Vector3"/>.
		/// </returns>
		public static explicit operator Vector2(Vector3 value)
		{
			return new Vector2(value.X, value.Y);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Vector3"/>.
		/// </summary>
		/// <param name="other">The <see cref="Vector3"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// Compairing is done without taking floating point error into account.
		/// </remarks>
		public bool Equals(Vector3 other)
		{
			return X == other.X 
				&& Y == other.Y 
				&& Z == other.Z;
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
			return obj is Vector3 && Equals((Vector3)obj);
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains 
		/// linear interpolation of the specified vectors.
		/// </summary>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="value1">The first <see cref="Vector3"/>.</param>
		/// <param name="value2">The second <see cref="Vector3"/>.</param>
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
		/// Multiplies the components of two <see cref="Vector3"/> instances by each other.
		/// </summary>
		/// <param name="left">Source <see cref="Vector3"/> on the left of the mul sign.</param>
		/// <param name="right">Source <see cref="Vector3"/> on the right of the mul sign.</param>
		/// <returns>The result of the <see cref="Vector3"/> multiplication.</returns>
		public static Vector3 operator *(Vector3 left, Vector3 right)
		{
			return new Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		/// <summary>
		/// Divides the components of the <see cref="Vector3"/> 
		/// by the components of another <see cref="Vector3"/>.
		/// </summary>
		/// <param name="left">Source <see cref="Vector3"/> on the left of the div sign.</param>
		/// <param name="right">Divisor <see cref="Vector3"/> on the right of the div sign.</param>
		/// <returns>The result of the <see cref="Vector3"/> divide.</returns>
		public static Vector3 operator /(Vector3 left, Vector3 right)
		{
			return new Vector3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		/// <summary>
		/// Divides the components of the <see cref="Vector3"/> by a scalar.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/> on the left of the div sign.</param>
		/// <param name="divider">Divisor scalar on the right of the div sign.</param>
		/// <returns>The result of dividing the <see cref="Vector3"/> by a scalar.</returns>
		public static Vector3 operator /(Vector3 value, float divider)
		{
			return new Vector3(value.X / divider, value.Y / divider, value.Z / divider);
		}

		/// <summary>
		/// Multiplies the components of two <see cref="Vector3"/> instances by each other.
		/// </summary>
		/// <param name="left">Source <see cref="Vector3"/> on the left of the mul sign.</param>
		/// <param name="right">Source <see cref="Vector3"/> on the right of the mul sign.</param>
		/// <returns>The result of the <see cref="Vector3"/> multiplication.</returns>
		public static Vector3 Scale(Vector3 left, Vector3 right)
		{
			return new Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		/// <summary>
		/// Compares whether two <see cref="Vector3"/> instances are equal.
		/// </summary>
		/// <param name="left"><see cref="Vector3"/> instance on the left of the equal sign.</param>
		/// <param name="right"><see cref="Vector3"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// Compairing is done without taking floating point error into account.
		/// </remarks>
		public static bool operator ==(Vector3 left, Vector3 right)
		{
			return left.X == right.X 
				&& left.Y == right.Y 
				&& left.Z == right.Z;
		}

		/// <summary>
		/// Compares whether two <see cref="Vector3"/> instances are not equal.
		/// </summary>
		/// <param name="left"><see cref="Vector3"/> instance on the left of the not equal sign.</param>
		/// <param name="right"><see cref="Vector3"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
		/// <remarks>
		/// Compairing is done without taking floating point error into account.
		/// </remarks>
		public static bool operator !=(Vector3 left, Vector3 right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Multiplies the components of <see cref="Vector3"/> by a scalar.
		/// </summary>
		/// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
		/// <param name="value">Source <see cref="Vector3"/> on the right of the mul sign.</param>
		/// <returns>The result of the <see cref="Vector3"/> multiplication with a scalar.</returns>
		public static Vector3 operator *(float scaleFactor, Vector3 value)
		{
			return new Vector3(scaleFactor * value.X, scaleFactor * value.Y, scaleFactor * value.Z);
		}

		/// <summary>
		/// Multiplies the components of <see cref="Vector3"/> by a scalar.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/> on the right of the mul sign.</param>
		/// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
		/// <returns>The result of the <see cref="Vector3"/> multiplication with a scalar.</returns>
		public static Vector3 operator *(Vector3 value, float scaleFactor)
		{
			return new Vector3(scaleFactor * value.X, scaleFactor * value.Y, scaleFactor * value.Z);
		}

		/// <summary>
		/// Adds the components of two <see cref="Vector3"/> instances.
		/// </summary>
		/// <param name="left">Source <see cref="Vector3"/> on the left of the add sign.</param>
		/// <param name="right">Source <see cref="Vector3"/> on the right of the add sign.</param>
		/// <returns>The result of the <see cref="Vector3"/> sum.</returns>
		public static Vector3 operator +(Vector3 left, Vector3 right)
		{
			return new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		/// <summary>
		/// Subtracts the components of another <see cref="Vector3"/> 
		/// from components of the <see cref="Vector3"/>.
		/// </summary>
		/// <param name="left">Source <see cref="Vector3"/> on the left of the sub sign.</param>
		/// <param name="right">Divisor <see cref="Vector3"/> on the right of the sub sign.</param>
		/// <returns>The result of the <see cref="Vector3"/> subtract.</returns>
		public static Vector3 operator -(Vector3 left, Vector3 right)
		{
			return new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		/// <summary>
		/// Inverts values in the specified <see cref="Vector3"/>.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/> on the right of the sub sign.</param>
		/// <returns>Result of the inversion.</returns>
		public static Vector3 operator -(Vector3 value)
		{
			return new Vector3(-value.X, -value.Y, -value.Z);
		}

		/// <summary>
		/// Returns a dot product of two <see cref="Vector3"/> instances.
		/// </summary>
		/// <param name="value1">The first <see cref="Vector3"/>.</param>
		/// <param name="value2">The second <see cref="Vector3"/>.</param>
		/// <returns>The dot product of two <see cref="Vector3"/> instances.</returns>
		public static float DotProduct(Vector3 value1, Vector3 value2)
		{
			return value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
		}

		/// <summary>
		/// Returns a cross product of two <see cref="Vector3"/> instances.
		/// </summary>
		/// <param name="value1">The first <see cref="Vector3"/>.</param>
		/// <param name="value2">The second <see cref="Vector3"/>.</param>
		/// <returns>The cross product of two <see cref="Vector3"/> instances.</returns>
		public static Vector3 CrossProduct(Vector3 value1, Vector3 value2)
		{
			return new Vector3(
				value1.Y * value2.Z - value1.Z * value2.Y,
				value1.Z * value2.X - value1.X * value2.Z,
				value1.X * value2.Y - value1.Y * value2.X
			);
		}

		/// <summary>
		/// Turns this <see cref="Vector3"/> to a unit vector with the same direction.
		/// </summary>
		public void Normalize()
		{
			var length = Length;
			if (length > 0)
			{
				X /= length;
				Y /= length;
				Z /= length;
			}
		}

		/// <summary>
		/// Returns this <see cref="Vector3"/> as a unit vector with the same direction.
		/// </summary>
		public Vector3 Normalized
		{
			get
			{
				var v = new Vector3(X, Y, Z);
				v.Normalize();
				return v;
			}
		}

		/// <summary>
		/// Returns specified <see cref="Vector3"/> as a unit vector with the same direction.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/>.</param>
		/// <returns>Source <see cref="Vector3"/> as a unit vector with the same direction.</returns>
		public static Vector3 Normalize(Vector3 value)
		{
			return value.Normalized;
		}

		/// <summary>
		/// Returns the length of this <see cref="Vector3"/>.
		/// </summary>
		/// <returns>The length of this <see cref="Vector3"/>.</returns>
		public float Length
		{
			get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); }
		}

		/// <summary>
		/// Returns the squared length of this <see cref="Vector3"/>.
		/// </summary>
		/// <returns>The squared length of this <see cref="Vector3"/>.</returns>
		public float SquaredLength
		{
			get { return X * X + Y * Y + Z * Z; }
		}

		/// <summary>
		/// Returns the <see cref="String"/> representation of this <see cref="Vector3"/> in the format:
		/// "[<see cref="X"/>], [<see cref="Y"/>], [<see cref="Z"/>]".
		/// </summary>
		/// <returns>The <see cref="String"/> representation of this <see cref="Vector3"/>.</returns>
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", X, Y, Z);
		}
	}
}