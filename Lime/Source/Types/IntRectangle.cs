using System;
using ProtoBuf;

namespace Lime
{
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct IntRectangle : IEquatable<IntRectangle>
	{
		[ProtoMember(1)]
		public IntVector2 A;

		[ProtoMember(2)]
		public IntVector2 B;
		
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

		public static explicit operator Rectangle(IntRectangle r)
		{
			return new Rectangle(r.Left, r.Top, r.Right, r.Bottom);
		}

		public Lime.IntRectangle Normalize()
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

		public static bool operator ==(IntRectangle lhs, IntRectangle rhs)
		{
			return lhs.A == rhs.A && lhs.B == rhs.B;
		}

		public static bool operator !=(IntRectangle lhs, IntRectangle rhs)
		{
			return lhs.A != rhs.A || lhs.B != rhs.B;
		}

		bool IEquatable<IntRectangle>.Equals(IntRectangle other)
		{
			return A.Equals(other.A) && B.Equals(other.B);
		}
		
		public override bool Equals(object o)
		{
			var rhs = (IntRectangle)o;
			return A == rhs.A && B == rhs.B;
		}

		public override int GetHashCode()
		{
			return A.GetHashCode() ^ B.GetHashCode();
		}

		public bool Contains(IntVector2 v)
		{
			return (v.X >= A.X) && (v.Y >= A.Y) && (v.X < B.X) && (v.Y < B.Y);
		}
		
		public override string ToString()
		{
			return String.Format("{0}, {1}, {2}, {3}", A.X, A.Y, B.X, B.Y);
		}
	}
}
