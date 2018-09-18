using System;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Representation of integer 2D rectangles.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[YuzuCompact]
	public struct IntRectangle : IEquatable<IntRectangle>
	{
		/// <summary>
		/// Left-top corner of this rectangle.
		/// </summary>
		[YuzuMember("0")]
		public IntVector2 A;

		/// <summary>
		/// Right-bottom corner of this rectangle.
		/// </summary>
		/// <remarks>Rectangle doesn't contain this point.</remarks>
		[YuzuMember("1")]
		public IntVector2 B;

		/// <summary>
		/// Returns a rectangle with both corners in 0, 0.
		/// </summary>
		public static readonly IntRectangle Empty = new IntRectangle();

		public IntRectangle(int left, int top, int right, int bottom)
		{
			A.X = left;
			A.Y = top;
			B.X = right;
			B.Y = bottom;
		}

		public IntRectangle(IntVector2 a, IntVector2 b)
		{
			A = a;
			B = b;
		}

		public static explicit operator Rectangle(IntRectangle value)
		{
			return new Rectangle(value.Left, value.Top, value.Right, value.Bottom);
		}

		public static explicit operator WindowRect(IntRectangle value)
		{
			return new WindowRect { X = value.Left, Y = value.Top, Width = value.Width, Height = value.Height };
		}

		/// <summary>
		/// Returns this rectangle with swapped coordinates
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

		public int Width {
			get { return B.X - A.X; }
			set { B.X = A.X + value; }
		}

		public int Height {
			get { return B.Y - A.Y; }
			set { B.Y = A.Y + value; }
		}

		public IntVector2 Size {
			get { return B - A; }
		}

		public int Left { get { return A.X; } set { A.X = value; } }

		public int Top { get { return A.Y; } set { A.Y = value; } }

		public int Right { get { return B.X; } set { B.X = value; } }

		public int Bottom { get { return B.Y; } set { B.Y = value; } }

		public IntVector2 Center { get { return new IntVector2((A.X + B.X) / 2, (A.Y + B.Y) / 2); } }

		/// <summary>
		/// Creates a new <see cref="IntRectangle"/> that has coordinates
		/// of this rectangle shifted by specified value.
		/// </summary>
		public IntRectangle OffsetBy(IntVector2 value)
		{
			return new IntRectangle(A + value, B + value);
		}

		public static bool operator ==(IntRectangle lhs, IntRectangle rhs)
		{
			return lhs.A == rhs.A && lhs.B == rhs.B;
		}

		public static bool operator !=(IntRectangle lhs, IntRectangle rhs)
		{
			return lhs.A != rhs.A || lhs.B != rhs.B;
		}

		public bool Equals(IntRectangle other)
		{
			return A.Equals(other.A) && B.Equals(other.B);
		}

		public override bool Equals(object obj)
		{
			return obj is IntRectangle && Equals((IntRectangle)obj);
		}

		public override int GetHashCode()
		{
			return A.GetHashCode() ^ B.GetHashCode();
		}

		public bool Contains(IntVector2 value)
		{
			return value.X >= A.X
				&& value.Y >= A.Y
				&& value.X < B.X
				&& value.Y < B.Y;
		}

		/// <summary>
		/// Returns the string representation of this <see cref="IntRectangle"/> in the format:
		/// "A.X, A.Y, B.X, B.Y".
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}, {3}", A.X, A.Y, B.X, B.Y);
		}

		/// <summary>
		/// Creates a new <see cref="IntRectangle"/> that contains
		/// overlapping region of two other rectangles.
		/// </summary>
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
