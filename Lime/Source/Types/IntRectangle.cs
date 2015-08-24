using System;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Representation of integer 2D rectangles. 
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct IntRectangle : IEquatable<IntRectangle>
	{
		/// <summary>
		/// Left-top corner of this <see cref="IntRectangle"/>.
		/// </summary>
		[ProtoMember(1)]
		public IntVector2 A;

		/// <summary>
		/// Right-bottom corner of this <see cref="IntRectangle"/>.
		/// </summary>
		/// <remarks>Rectangle doesn't contain this point.</remarks>
		[ProtoMember(2)]
		public IntVector2 B;

		/// <summary>
		/// Returns a <see cref="IntRectangle"/> with both corners in 0, 0.
		/// </summary>
		public static readonly IntRectangle Empty = new IntRectangle();

		/// <summary>
		/// Constructs a 2D rectangle with specified coordinates 
		/// of left, top, right and bottom borders.
		/// </summary>
		/// <param name="left">The X coordinate of left border in 2D-space.</param>
		/// <param name="top">The Y coordinate of top border in 2D-space.</param>
		/// <param name="right">The X coordinate of right border in 2D-space.</param>
		/// <param name="bottom">The Y coordinate of bottom border in 2D-space.</param>
		public IntRectangle(int left, int top, int right, int bottom)
		{
			A.X = left;
			A.Y = top;
			B.X = right;
			B.Y = bottom;
		}

		/// <summary>
		/// Constructs a 2D rectangle with specified coordinates 
		/// of left-top and right-bottom corners.
		/// </summary>
		/// <param name="a">Coordinates of left-top corner of rectangle in 2D-space.</param>
		/// <param name="b">Coordinates of right-bottom corner of rectangle in 2D-space.</param>
		public IntRectangle(IntVector2 a, IntVector2 b)
		{
			A = a;
			B = b;
		}

		/// <summary>
		/// Explicit cast from <see cref="IntRectangle"/> to <see cref="Rectangle"/>.
		/// </summary>
		/// <param name="value">Source <see cref="IntRectangle"/>.</param>
		/// <returns>
		/// New <see cref="Rectangle"/> with coordinates of corners 
		/// of source <see cref="IntRectangle"/>.
		/// </returns>
		public static explicit operator Rectangle(IntRectangle value)
		{
			return new Rectangle(value.Left, value.Top, value.Right, value.Bottom);
		}

		/// <summary>
		/// Explicit cast from <see cref="IntRectangle"/> to <see cref="WindowRect"/>.
		/// </summary>
		/// <param name="value">Source <see cref="IntRectangle"/>.</param>
		/// <returns>
		/// New <see cref="WindowRect"/> with coordinates of corners 
		/// of source <see cref="IntRectangle"/>.
		/// </returns>
		public static explicit operator WindowRect(IntRectangle value)
		{
			return new WindowRect { X = value.Left, Y = value.Top, Width = value.Width, Height = value.Height };
		}

		// TODO: Remove after 23.09.15, implement self Normalize() after that
		[Obsolete("Use \"Normalized\" property instead", true)]
		public IntRectangle Normalize()
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
			return rect;
		}

		/// <summary>
		/// Returns this <see cref="IntRectangle"/> with swapped coordinates 
		/// of borders if width or height is negative.
		/// </summary>
		/// <remarks>
		/// Width or height can be negative if coordinates of borders are mixed up.
		/// This property returns new <see cref="IntRectangle"/> with width and height
		/// that are guaranteed to be positive.
		/// </remarks>
		public IntRectangle Normalized
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
				return rect;
			}
		}

		/// <summary>
		/// The width of this <see cref="IntRectangle"/>.
		/// </summary>
		public int Width {
			get { return B.X - A.X; }
			set { B.X = A.X + value; }
		}

		/// <summary>
		/// The height of this <see cref="IntRectangle"/>.
		/// </summary>
		public int Height {
			get { return B.Y - A.Y; }
			set { B.Y = A.Y + value; }
		}

		/// <summary>
		/// The width-height coordinates of this <see cref="IntRectangle"/>.
		/// </summary>
		public IntVector2 Size { 
			get { return B - A; }
		}

		/// <summary>
		/// The X coordinate of left border of this <see cref="IntRectangle"/>.
		/// </summary>
		public int Left { get { return A.X; } set { A.X = value; } }

		/// <summary>
		/// The Y coordinate of top border of this <see cref="IntRectangle"/>.
		/// </summary>
		public int Top { get { return A.Y; } set { A.Y = value; } }

		/// <summary>
		/// The X coordinate of right border of this <see cref="IntRectangle"/>.
		/// </summary>
		public int Right { get { return B.X; } set { B.X = value; } }

		/// <summary>
		/// The Y coordinate of bottom border of this <see cref="IntRectangle"/>.
		/// </summary>
		public int Bottom { get { return B.Y; } set { B.Y = value; } }

		/// <summary>
		/// Coordinates of center point of this <see cref="IntRectangle"/>.
		/// </summary>
		public IntVector2 Center { get { return new IntVector2((A.X + B.X) / 2, (A.Y + B.Y) / 2); } }

		/// <summary>
		/// Creates a new <see cref="IntRectangle"/> that has coordinates 
		/// of this <see cref="IntRectangle"/> shifted by specified value.
		/// </summary>
		/// <param name="value">Offset of result <see cref="IntRectangle"/>.</param>
		/// <returns>
		/// New <see cref="IntRectangle"/> that has coordinates 
		/// of this <see cref="IntRectangle"/> shifted by specified value.
		/// </returns>
		public IntRectangle OffsetBy(IntVector2 value)
		{
			return new IntRectangle(A + value, B + value);
		}

		/// <summary>
		/// Compares whether two <see cref="IntRectangle"/> instances are equal.
		/// </summary>
		/// <param name="lhs"><see cref="IntRectangle"/> instance on the left of the equal sign.</param>
		/// <param name="rhs"><see cref="IntRectangle"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(IntRectangle lhs, IntRectangle rhs)
		{
			return lhs.A == rhs.A && lhs.B == rhs.B;
		}

		/// <summary>
		/// Compares whether two <see cref="IntRectangle"/> instances are not equal.
		/// </summary>
		/// <param name="lhs"><see cref="IntRectangle"/> instance on the left of the not equal sign.</param>
		/// <param name="rhs"><see cref="IntRectangle"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
		public static bool operator !=(IntRectangle lhs, IntRectangle rhs)
		{
			return lhs.A != rhs.A || lhs.B != rhs.B;
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="IntRectangle"/>.
		/// </summary>
		/// <param name="other">The <see cref="IntRectangle"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(IntRectangle other)
		{
			return A.Equals(other.A) && B.Equals(other.B);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			return obj is IntRectangle && Equals((IntRectangle)obj);
		}

		/// <summary>
		/// Gets the hash code of this <see cref="IntRectangle"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="IntRectangle"/>.</returns>
		public override int GetHashCode()
		{
			return A.GetHashCode() ^ B.GetHashCode();
		}

		/// <summary>
		/// Gets whether or not the provided point lies within the bounds 
		/// of this <see cref="IntRectangle"/>.
		/// </summary>
		/// <param name="value">The coordinates of point to check for containment.</param>
		/// <returns>
		/// <c>true</c> if the provided point lies inside this <see cref="IntRectangle"/>; 
		/// <c>false</c> otherwise.
		/// </returns>
		public bool Contains(IntVector2 value)
		{
			return value.X >= A.X
				&& value.Y >= A.Y 
				&& value.X < B.X 
				&& value.Y < B.Y;
		}

		// TODO: Maybe swap to return string.Format("{0}, {1}", A.ToString(), B.ToString());?
		/// <summary>
		/// Returns the <see cref="String"/> representation of this <see cref="IntRectangle"/> in the format:
		/// "<see cref="A.X"/>, <see cref="A.Y"/>, <see cref="B.X"/>, <see cref="B.Y"/>".
		/// </summary>
		/// <returns>The <see cref="String"/> representation of this <see cref="IntRectangle"/>.</returns>
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}, {3}", A.X, A.Y, B.X, B.Y);
		}

		/// <summary>
		/// Creates a new <see cref="IntRectangle"/> that contains 
		/// overlapping region of two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="IntRectangle"/>.</param>
		/// <param name="value2">The second <see cref="IntRectangle"/>.</param>
		/// <returns>
		/// Overlapping region of the two rectangles if they overlaps; 
		/// <see cref="IntRectangle.Empty"/> otherwise.
		/// </returns>
		public static IntRectangle Intersect(IntRectangle value1, IntRectangle value2)
		{
			var x0 = Math.Max(value1.A.X, value2.A.X);
			var x1 = Math.Min(value1.B.X, value2.B.X);
			var y0 = Math.Max(value1.A.Y, value2.A.Y);
			var y1 = Math.Min(value1.B.Y, value2.B.Y);
			return x1 >= x0 && y1 >= y0 ? new IntRectangle(x0, y0, x1, y1) : Empty;
		}
	}
}
