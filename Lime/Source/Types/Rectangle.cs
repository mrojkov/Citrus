using System;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Representation of 2D rectangles. 
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct Rectangle : IEquatable<Rectangle>
	{
		/// <summary>
		/// Left-top corner of this <see cref="Rectangle"/>.
		/// </summary>
		[ProtoMember(1)]
		public Vector2 A;

		/// <summary>
		/// Right-bottom corner of this <see cref="Rectangle"/>.
		/// </summary>
		/// <remarks>Rectangle doesn't contain this point.</remarks>
		[ProtoMember(2)]
		public Vector2 B;

		/// <summary>
		/// Returns a <see cref="Rectangle"/> with both corners in 0, 0.
		/// </summary>
		public static readonly Rectangle Empty = new Rectangle();

		/// <summary>
		/// Constructs a 2D rectangle with specified coordinates 
		/// of left, top, right and bottom borders.
		/// </summary>
		/// <param name="left">The X coordinate of left border in 2D-space.</param>
		/// <param name="top">The Y coordinate of top border in 2D-space.</param>
		/// <param name="right">The X coordinate of right border in 2D-space.</param>
		/// <param name="bottom">The Y coordinate of bottom border in 2D-space.</param>
		public Rectangle(float left, float top, float right, float bottom)
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
		public Rectangle(Vector2 a, Vector2 b)
		{
			A = a;
			B = b;
		}

		/// <summary>
		/// Explicit cast from <see cref="Rectangle"/> to <see cref="IntRectangle"/>.
		/// </summary>
		/// <param name="value">Source <see cref="Rectangle"/>.</param>
		/// <returns>
		/// New <see cref="IntRectangle"/> with truncated coordinates of corners 
		/// of source <see cref="Rectangle"/>.
		/// </returns>
		public static explicit operator IntRectangle(Rectangle value)
		{
			return new IntRectangle((int)value.Left, (int)value.Top, (int)value.Right, (int)value.Bottom);
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
			return obj is Rectangle && Equals((Rectangle)obj);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Rectangle"/>.
		/// </summary>
		/// <param name="other">The <see cref="Rectangle"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// Compairing is done without taking floating point error into account.
		/// </remarks>
		public bool Equals(Rectangle other)
		{
			return A.Equals(other.A) && B.Equals(other.B);
		}

		/// <summary>
		/// Compares whether two <see cref="Rectangle"/> instances are equal.
		/// </summary>
		/// <param name="lhs"><see cref="Rectangle"/> instance on the left of the equal sign.</param>
		/// <param name="rhs"><see cref="Rectangle"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// Compairing is done without taking floating point error into account.
		/// </remarks>
		public static bool operator ==(Rectangle lhs, Rectangle rhs)
		{
			return lhs.Equals(rhs);
		}

		/// <summary>
		/// Compares whether two <see cref="Rectangle"/> instances are not equal.
		/// </summary>
		/// <param name="lhs"><see cref="Rectangle"/> instance on the left of the not equal sign.</param>
		/// <param name="rhs"><see cref="Rectangle"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
		/// <remarks>
		/// Compairing is done without taking floating point error into account.
		/// </remarks>
		public static bool operator !=(Rectangle lhs, Rectangle rhs)
		{
			return !lhs.Equals(rhs);
		}

		/// <summary>
		/// The width of this <see cref="Rectangle"/>.
		/// </summary>
		public float Width
		{
			get { return B.X - A.X; }
			set { B.X = A.X + value; }
		}

		/// <summary>
		/// The height of this <see cref="Rectangle"/>.
		/// </summary>
		public float Height
		{
			get { return B.Y - A.Y; }
			set { B.Y = A.Y + value; }
		}

		/// <summary>
		/// The X coordinate of left border of this <see cref="Rectangle"/>.
		/// </summary>
		public float Left { get { return A.X; } set { A.X = value; } }

		/// <summary>
		/// The Y coordinate of top border of this <see cref="Rectangle"/>.
		/// </summary>
		public float Top { get { return A.Y; } set { A.Y = value; } }

		/// <summary>
		/// The X coordinate of right border of this <see cref="Rectangle"/>.
		/// </summary>
		public float Right { get { return B.X; } set { B.X = value; } }

		/// <summary>
		/// The Y coordinate of bottom border of this <see cref="Rectangle"/>.
		/// </summary>
		public float Bottom { get { return B.Y; } set { B.Y = value; } }

		/// <summary>
		/// Coordinates of center point of this <see cref="Rectangle"/>.
		/// </summary>
		public Vector2 Center { get { return (A + B) / 2; } }

		/// <summary>
		/// Swaps coordinates of borders if width or height is negative.
		/// </summary>
		/// <remarks>
		/// Width or height can be negative if coordinates of borders are mixed up.
		/// After this method width and height are guaranteed to be positive.
		/// </remarks>>
		public void Normalize()
		{
			if (A.X > B.X) {
				Toolbox.Swap(ref A.X, ref B.X);
			}
			if (A.Y > B.Y) {
				Toolbox.Swap(ref A.Y, ref B.Y);
			}
		}

		/// <summary>
		/// Gets whether or not the provided point lies within the bounds 
		/// of this <see cref="Rectangle"/>.
		/// </summary>
		/// <param name="value">The coordinates of point to check for containment.</param>
		/// <returns>
		/// <c>true</c> if the provided point lies inside this <see cref="Rectangle"/>; 
		/// <c>false</c> otherwise.
		/// </returns>
		public bool Contains(Vector2 value)
		{
			return (value.X >= A.X) && (value.Y >= A.Y) && (value.X < B.X) && (value.Y < B.Y);
		}

		/// <summary>
		/// The width-height coordinates of this <see cref="Rectangle"/>.
		/// </summary>
		public Vector2 Size { 
			get { return B - A; }
		}

		/// <summary>
		/// Creates a new <see cref="Rectangle"/> that contains 
		/// overlapping region of two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="Rectangle"/>.</param>
		/// <param name="value2">The second <see cref="Rectangle"/>.</param>
		/// <returns>
		/// Overlapping region of the two rectangles if they overlaps; 
		/// <see cref="Rectangle.Empty"/> otherwise.
		/// </returns>
		public static Rectangle Intersect(Rectangle value1, Rectangle value2)
		{
			var x0 = Math.Max(value1.A.X, value2.A.X);
			var x1 = Math.Min(value1.B.X, value2.B.X);
			var y0 = Math.Max(value1.A.Y, value2.A.Y);
			var y1 = Math.Min(value1.B.Y, value2.B.Y);
			return x1 >= x0 && y1 >= y0 ? new Rectangle(x0, y0, x1, y1) : Empty;
		}

		/// <summary>
		/// Creates a new <see cref="Rectangle"/> that covers both of two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="Rectangle"/>.</param>
		/// <param name="value2">The second <see cref="Rectangle"/>.</param>
		/// <returns>New <see cref="Rectangle"/> that covers both of two other rectangles.</returns>
		public static Rectangle Bounds(Rectangle value1, Rectangle value2)
		{
			return new Rectangle(
				Mathf.Min(value1.Left, value2.Left),
				Mathf.Min(value1.Top, value2.Top),
				Mathf.Max(value1.Right, value2.Right),
				Mathf.Max(value1.Bottom, value2.Bottom)
			);
		}

		/// <summary>
		/// Changes bounds of this <see cref="Rectangle"/> so it includes
		/// the specified <see cref="Vector2"/>.
		/// </summary>
		/// <param name="value">The <see cref="Vector2"/> to include.</param>
		/// <remarks>Does nothing, if this <see cref="Rectangle"/> already includes
		/// the specified <see cref="Vector2"/>.</remarks>
		public void IncludePoint(Vector2 value)
		{
			Left = Mathf.Min(value.X, Left);
			Right = Mathf.Max(value.X, Right);
			Top = Mathf.Min(value.Y, Top);
			Bottom = Mathf.Max(value.Y, Bottom);
		}

		// TODO: Maybe swap to return string.Format("{0}, {1}", A.ToString(), B.ToString());?
		/// <summary>
		/// Returns the <see cref="String"/> representation of this <see cref="Rectangle"/> in the format:
		/// "<see cref="A.X"/>, <see cref="A.Y"/>, <see cref="B.X"/>, <see cref="B.Y"/>".
		/// </summary>
		/// <returns>The <see cref="String"/> representation of this <see cref="Rectangle"/>.</returns>
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}, {3}", A.X, A.Y, B.X, B.Y);
		}

		/// <summary>
		/// Gets the hash code of this <see cref="Rectangle"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Rectangle"/>.</returns>
		public override int GetHashCode()
		{
			return A.GetHashCode() ^ B.GetHashCode();
		}

		/// <summary>
		/// Applies the transformation matrix to this <see cref="Rectangle"/>.
		/// </summary>
		/// <param name="value">The <see cref="Matrix32"/> to apply.</param>
		public Rectangle Transform(Matrix32 value)
		{
			return new Rectangle(A * value, B * value);
		}
	}
}