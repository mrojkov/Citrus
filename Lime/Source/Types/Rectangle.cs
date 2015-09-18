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
		/// Left-top corner of this rectangle.
		/// </summary>
		[ProtoMember(1)]
		public Vector2 A;

		/// <summary>
		/// Right-bottom corner of this rectangle.
		/// </summary>
		/// <remarks>Rectangle doesn't contain this point.</remarks>
		[ProtoMember(2)]
		public Vector2 B;

		/// <summary>
		/// Returns a rectangle with both corners in 0, 0.
		/// </summary>
		public static readonly Rectangle Empty = new Rectangle();
		
		public Rectangle(float left, float top, float right, float bottom)
		{
			A.X = left;
			A.Y = top;
			B.X = right;
			B.Y = bottom;
		}
		
		public Rectangle(Vector2 a, Vector2 b)
		{
			A = a;
			B = b;
		}
		
		public static explicit operator IntRectangle(Rectangle value)
		{
			return new IntRectangle((int)value.Left, (int)value.Top, (int)value.Right, (int)value.Bottom);
		}
		
		public override bool Equals(object obj)
		{
			return obj is Rectangle && Equals((Rectangle)obj);
		}
		
		public bool Equals(Rectangle other)
		{
			return A.Equals(other.A) && B.Equals(other.B);
		}
		
		public static bool operator ==(Rectangle lhs, Rectangle rhs)
		{
			return lhs.Equals(rhs);
		}
		
		public static bool operator !=(Rectangle lhs, Rectangle rhs)
		{
			return !lhs.Equals(rhs);
		}
		
		public float Width
		{
			get { return B.X - A.X; }
			set { B.X = A.X + value; }
		}
		
		public float Height
		{
			get { return B.Y - A.Y; }
			set { B.Y = A.Y + value; }
		}
		
		public float Left { get { return A.X; } set { A.X = value; } }
		
		public float Top { get { return A.Y; } set { A.Y = value; } }
		
		public float Right { get { return B.X; } set { B.X = value; } }
		
		public float Bottom { get { return B.Y; } set { B.Y = value; } }
		
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
		
		public bool Contains(Vector2 value)
		{
			return (value.X >= A.X) && (value.Y >= A.Y) && (value.X < B.X) && (value.Y < B.Y);
		}

		public Vector2 Size { 
			get { return B - A; }
		}

		/// <summary>
		/// Creates a new <see cref="Rectangle"/> that contains 
		/// overlapping region of two other rectangles.
		/// </summary>
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
		public void IncludePoint(Vector2 value)
		{
			Left = Mathf.Min(value.X, Left);
			Right = Mathf.Max(value.X, Right);
			Top = Mathf.Min(value.Y, Top);
			Bottom = Mathf.Max(value.Y, Bottom);
		}

		// TODO: Maybe swap to return string.Format("{0}, {1}", A.ToString(), B.ToString());?
		/// <summary>
		/// Returns the string representation of this <see cref="Rectangle"/> in the format:
		/// "A.X, A.Y, B.X, B.Y".
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}, {3}", A.X, A.Y, B.X, B.Y);
		}
		
		public override int GetHashCode()
		{
			return A.GetHashCode() ^ B.GetHashCode();
		}

		/// <summary>
		/// Applies the transformation matrix to this <see cref="Rectangle"/>.
		/// </summary>
		public Rectangle Transform(Matrix32 value)
		{
			return new Rectangle(A * value, B * value);
		}
	}
}