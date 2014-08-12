using System;
using ProtoBuf;

namespace Lime
{
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct Rectangle : IEquatable<Rectangle>
	{
		[ProtoMember(1)]
		public Vector2 A;

		[ProtoMember(2)]
		public Vector2 B;

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

		public static explicit operator IntRectangle(Rectangle r)
		{
			return new IntRectangle((int)r.Left, (int)r.Top, (int)r.Right, (int)r.Bottom);
		}

		public override bool Equals(object obj)
		{
			var rhs = (Rectangle)obj;
			return A.Equals(rhs.A) && B.Equals(rhs.B);
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

		public void Normalize()
		{
			if (A.X > B.X) {
				Toolbox.Swap(ref A.X, ref B.X);
			}
			if (A.Y > B.Y) {
				Toolbox.Swap(ref A.Y, ref B.Y);
			}
		}

		public bool Contains(Vector2 v)
		{
			return (v.X >= A.X) && (v.Y >= A.Y) && (v.X < B.X) && (v.Y < B.Y);
		}

		public Vector2 Size { 
			get { return B - A; }
		}

		public static Rectangle Intersect(Rectangle a, Rectangle b)
		{
			float x0 = Math.Max(a.A.X, b.A.X);
			float x1 = Math.Min(a.B.X, b.B.X);
			float y0 = Math.Max(a.A.Y, b.A.Y);
			float y1 = Math.Min(a.B.Y, b.B.Y);
			if (x1 >= x0 && y1 >= y0) {
				return new Rectangle(x0, y0, x1, y1);
			} else {
				return Empty;
			}
		}

		public static Rectangle Bounds(Rectangle a, Rectangle b)
		{
			return new Rectangle(
				Mathf.Min(a.Left, b.Left),
				Mathf.Min(a.Top, b.Top),
				Mathf.Max(a.Right, b.Right),
				Mathf.Max(a.Bottom, b.Bottom)
			);
		}
				
		public override string ToString()
		{
			return String.Format("{0}, {1}, {2}, {3}", A.X, A.Y, B.X, B.Y);
		}

		public override int GetHashCode()
		{
			return A.GetHashCode() ^ B.GetHashCode();
		}
	}
}