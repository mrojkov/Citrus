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
		
		public Rectangle (float left, float top, float right, float bottom)
		{
			A.X = left;
			A.Y = top;
			B.X = right;
			B.Y = bottom;
		}

		public Rectangle (Vector2 a, Vector2 b)
		{
			A = a;
			B = b;
		}
		
		bool IEquatable<Rectangle>.Equals (Rectangle other)
		{
			return A.Equals (other.A) && B.Equals (other.B);
		}
		
		public float Width {
			get {
				return B.X - A.X;
			}
			set {
				B.X = A.X + value;
			}
		}

		public float Height {
			get {
				return B.Y - A.Y;
			}
			set {
				B.Y = A.Y + value;
			}
		}
		
		bool Contains (Vector2 v)
		{
			return (v.X >= A.X) && (v.Y >= A.Y) && (v.X < B.X) && (v.Y < B.Y);
		}
		
		public override string ToString ()
		{
			return String.Format ("({0}, {1}, {2}, {3})", A.X, A.Y, B.X, B.Y);
		}

	}
}