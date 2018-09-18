using System;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Representation of 2D rectangles.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[YuzuCompact]
	public struct Rectangle : IEquatable<Rectangle>
	{
		[YuzuMember("0")]
		public float AX;

		[YuzuMember("1")]
		public float AY;

		[YuzuMember("2")]
		public float BX;

		[YuzuMember("3")]
		public float BY;

		/// <summary>
		/// Returns a rectangle with both corners in 0, 0.
		/// </summary>
		public static readonly Rectangle Empty = new Rectangle();

		/// <summary>
		/// Left-top corner of this rectangle.
		/// </summary>
		public Vector2 A { get { return new Vector2(AX, AY); } set { AX = value.X; AY = value.Y; } }

		/// <summary>
		/// Right-bottom corner of this rectangle.
		/// </summary>
		public Vector2 B { get { return new Vector2(BX, BY); } set { BX = value.X; BY = value.Y; } }

		public Rectangle(float left, float top, float right, float bottom)
		{
			AX = left;
			AY = top;
			BX = right;
			BY = bottom;
		}

		public Rectangle(Vector2 a, Vector2 b)
		{
			AX = a.X;
			AY = a.Y;
			BX = b.X;
			BY = b.Y;
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
			get { return BX - AX; }
			set { BX = AX + value; }
		}

		public float Height
		{
			get { return BY - AY; }
			set { BY = AY + value; }
		}

		public float Left { get { return AX; } set { AX = value; } }

		public float Top { get { return AY; } set { AY = value; } }

		public float Right { get { return BX; } set { BX = value; } }

		public float Bottom { get { return BY; } set { BY = value; } }

		public Vector2 Center { get { return (A + B) / 2; } }

		/// <summary>
		/// Returns this rectangle with swapped coordinates
		/// of borders if width or height is negative.
		/// </summary>
		/// <remarks>
		/// Width or height can be negative if coordinates of borders are mixed up.
		/// This property returns new <see cref="Rectangle"/> with width and height
		/// that are guaranteed to be positive.
		/// </remarks>
		public Rectangle Normalized
		{
			get
			{
				var rect = this;
				if (Width < 0) {
					rect.AX = BX;
					rect.BX = AX;
				}
				if (Height < 0) {
					rect.AY = BY;
					rect.BY = AY;
				}
				return rect;
			}
		}

		public bool Contains(Vector2 value)
		{
			return AX < BX ? CheckContains(value, A, B) : CheckContains(value, B, A);
		}

		private static bool CheckContains(Vector2 value, Vector2 a, Vector2 b)
		{
			if (a.Y > b.Y) {
				return (value.X >= a.X) && (value.Y <= a.Y) && (value.X < b.X) && (value.Y > b.Y);
			} else {
				return (value.X >= a.X) && (value.Y >= a.Y) && (value.X < b.X) && (value.Y < b.Y);
			}
		}

		public Vector2 Size => B - A;

		/// <summary>
		/// Creates a new <see cref="Rectangle"/> that contains
		/// overlapping region of two other rectangles.
		/// </summary>
		public static Rectangle Intersect(Rectangle value1, Rectangle value2)
		{
			var x0 = Math.Max(value1.AX, value2.AX);
			var x1 = Math.Min(value1.BX, value2.BX);
			var y0 = Math.Max(value1.AY, value2.AY);
			var y1 = Math.Min(value1.BY, value2.BY);
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
		/// Creates a new <see cref="Rectangle"/> that includes
		/// the specified <see cref="Vector2"/>.
		/// </summary>
		public Rectangle IncludingPoint(Vector2 value)
		{
			return new Rectangle(
				Mathf.Min(value.X, Left),
				Mathf.Min(value.Y, Top),
				Mathf.Max(value.X, Right),
				Mathf.Max(value.Y, Bottom)
			);
		}

		public Rectangle ExpandedBy(Thickness padding)
		{
			return new Rectangle(
				Left - padding.Left,
				Top - padding.Top,
				Right + padding.Right,
				Bottom + padding.Bottom
			);
		}

		public Rectangle ShrinkedBy(Thickness padding) =>
			new Rectangle(
				Left + padding.Left,
				Top + padding.Top,
				Right - padding.Right,
				Bottom - padding.Bottom
			);

		/// <summary>
		/// Returns the string representation of this <see cref="Rectangle"/> in the format:
		/// "A.X, A.Y, B.X, B.Y".
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}, {3}", AX, AY, BX, BY);
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

		public Quadrangle ToQuadrangle()
		{
			return new Quadrangle {
				V1 = A,
				V2 = new Vector2(BX, AY),
				V3 = B,
				V4 = new Vector2(AX, BY)
			};
		}
	}
}
