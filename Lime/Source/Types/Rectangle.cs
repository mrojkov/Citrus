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